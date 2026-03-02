using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;


namespace WhiteLagoon.Web.Controllers;

// 1. The Primary Constructor defines 'unitOfWork' for the whole class
public class BookingController(IUnitOfWork unitOfWork) : Controller
{
    // 2. No manual field declaration needed
    // 3. No traditional constructor block needed
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ApplicationUser user = unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

        Booking booking = new()
        {
            VillaId = villaId,
            Villa = unitOfWork.VillaRepository.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
            CheckInDate = checkInDate,
            Nights = nights,
            CheckOutDate = checkInDate.AddDays(nights),
            UserId = userId,
            Phone = user.PhoneNumber,
            Email = user.Email,
            Name = user.Name
        };
        booking.TotalCost = booking.Villa.Price * nights;
        return View(booking);
    }

    [Authorize]
    [HttpPost]
    public IActionResult FinalizeBooking(Booking booking)
    {
        var villa = unitOfWork.VillaRepository.Get(u => u.Id == booking.VillaId);
        booking.TotalCost = villa.Price * booking.Nights;

        booking.Status = SD.Status_Pending;
        booking.BookingDate = DateTime.Now;

        var villaNumbersList = unitOfWork.VillaNumberRepository.GetAll().ToList();
        var bookedVillas = unitOfWork.BookingRepository.GetAll(u => u.Status == SD.Status_Approved ||
        u.Status == SD.Status_CheckedIn).ToList();

        int roomAvailable = SD.VillaRoomsAvailable_Count
            (villa.Id, villaNumbersList, booking.CheckInDate, booking.Nights, bookedVillas);

        if (roomAvailable == 0)
        {
            TempData["error"] = "Room has been sold out!";
            //no rooms available
            return RedirectToAction(nameof(FinalizeBooking), new
            {
                villaId = booking.VillaId,
                checkInDate = booking.CheckInDate,
                nights = booking.Nights
            });
        }


        unitOfWork.BookingRepository.Add(booking);
        unitOfWork.Save();

        var successUrl = Url.Action(nameof(BookingConfirmation), "Booking", new { bookingId = booking.Id }, Request.Scheme);
        var cancelUrl = Url.Action(nameof(FinalizeBooking), "Booking", new { villaId = booking.VillaId, checkInDate = booking.CheckInDate, nights = booking.Nights }, Request.Scheme);


        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        /*
         * if we have a shopping cart with multiple items, we will add them here with a loop
         */
        options.LineItems.Add(
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = SD.Currency_Usd,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string> { domain + villa.ImageUrl }>,
                    }
                },
                Quantity = 1 //we are booking one villa at a time
            });

        var service = new SessionService();
        Session session = service.Create(options);

        unitOfWork.BookingRepository.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
        unitOfWork.Save();
        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    [Authorize]
    public IActionResult BookingConfirmation(int bookingId)
    {
        Booking bookingFromDb = unitOfWork.BookingRepository.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

        if (bookingFromDb.Status == SD.Status_Pending)
        {
            // if it is pending, we must confirm if payment was successful or not.
            var service = new SessionService();
            Session session = service.Get(bookingFromDb.StripeSessionId);
            if (session.PaymentStatus.ToLower() == SD.Payment_Status_Paid)
            {
                unitOfWork.BookingRepository.UpdateStatus(bookingFromDb.Id, SD.Status_Approved, 0);
                unitOfWork.BookingRepository.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                unitOfWork.Save();
            }
        }
        return View(bookingId);
    }

    [Authorize]
    public IActionResult BookingDetails(int bookingId)
    {
        Booking bookingFromDb = unitOfWork.BookingRepository.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

        if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.Status_Approved)
        {
            var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

            bookingFromDb.VillaNumbers = unitOfWork.VillaNumberRepository.GetAll(u => u.VillaId == bookingFromDb.VillaId
            && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
        }

        return View(bookingFromDb);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CheckIn(Booking booking)
    {
        unitOfWork.BookingRepository.UpdateStatus(booking.Id, SD.Status_CheckedIn, booking.VillaNumber);
        unitOfWork.Save();
        TempData["Success"] = "Booking Updated Successfully.";
        return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CheckOut(Booking booking)
    {
        unitOfWork.BookingRepository.UpdateStatus(booking.Id, SD.Status_Completed, booking.VillaNumber);
        unitOfWork.Save();
        TempData["Success"] = "Booking Completed Successfully.";
        return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CancelBooking(Booking booking)
    {
        unitOfWork.BookingRepository.UpdateStatus(booking.Id, SD.Status_Cancelled, 0);
        unitOfWork.Save();
        TempData["Success"] = "Booking Cancelled Successfully.";
        return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
    }

    private List<int> AssignAvailableVillaNumberByVilla(int villaId)
    {
        List<int> availableVillaNumbers = new();

        var villaNumbers = unitOfWork.VillaNumberRepository.GetAll(u => u.VillaId == villaId);

        var checkedInVilla = unitOfWork.BookingRepository.GetAll(u => u.VillaId == villaId && u.Status == SD.Status_CheckedIn)
            .Select(u => u.VillaNumber);

        foreach (var villaNumber in villaNumbers)
        {
            if (!checkedInVilla.Contains(villaNumber.Villa_Number))
            {
                availableVillaNumbers.Add(villaNumber.Villa_Number);
            }
        }
        return availableVillaNumbers;
    }

    #region API Calls
    [HttpGet]
    [Authorize]
    public IActionResult GetAll(string status)
    {
        IEnumerable<Booking> objBookings;
        if (User.IsInRole(SD.Role_Admin))
        {
            objBookings = unitOfWork.BookingRepository.GetAll(includeProperties: "User,Villa");
        }
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            objBookings = unitOfWork.BookingRepository
                .GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
        }
        if (!string.IsNullOrEmpty(status))
        {
            objBookings = objBookings.Where(u => u.Status == status);
        }


        return Json(new { data = objBookings });
    }

    #endregion
}