using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(Booking entity)
    {
        _context.Update(entity);
    }

    public void UpdateStatus(int bookingId, string bookingStatus)
    {
        var bookingFromDb = _context.Bookings.FirstOrDefault(u => u.Id == bookingId);
        if (bookingFromDb != null)
        {
            bookingFromDb.Status = bookingStatus;
            if (bookingStatus == SD.Status_CheckedIn)
            {
                bookingFromDb.ActualCheckInDate = DateTime.Now;
            }
            if (bookingStatus == SD.Status_Completed)
            {
                bookingFromDb.ActualCheckOutDate = DateTime.Now;
            }
            _context.SaveChanges();
        }

    }

    public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
    {
        var bookingFromDb = _context.Bookings.FirstOrDefault(u => u.Id == bookingId);
        if (bookingFromDb != null)
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                bookingFromDb.StripeSessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                bookingFromDb.StripePaymentIntentId = paymentIntentId;
                bookingFromDb.PaymentDate = DateTime.Now;
            }

        }
    }
}
