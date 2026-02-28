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

        unitOfWork.BookingRepository.Add(booking);
        unitOfWork.Save();

        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}.html",
            CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
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
                unitOfWork.BookingRepository.UpdateStatus(bookingFromDb.Id, SD.Status_Approved);
                unitOfWork.BookingRepository.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                unitOfWork.Save();
            }
        }
        return View(bookingId);
    }
}