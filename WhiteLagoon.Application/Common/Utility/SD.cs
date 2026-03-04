using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Common.Utility;

public class SD
{
    public const string Role_Customer = "Customer";
    public const string Role_Admin = "Admin";
    public const string Role_Employee = "Employee";

    public const string Status_Pending = "Pending";
    public const string Status_Approved = "Approved";
    public const string Status_CheckedIn = "CheckedIn";
    public const string Status_Completed = "Completed";
    public const string Status_Cancelled = "Cancelled";
    public const string Status_Refunded = "Refunded";
    public const string Status_InProcess = "Processing";
    public const string Status_ReadyForPickup = "Ready for Pickup";

    public const string Currency_Usd = "usd";

    public const string Payment_Status_Paid = "paid";

    public static int VillaRoomsAvailable_Count(int villaId,
            List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights,
           List<Booking> bookings)
    {
        List<int> bookingInDate = new();
        int finalAvailableRoomForAllNights = int.MaxValue;
        var roomsInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count();

        for (int i = 0; i < nights; i++)
        {
            var villasBooked = bookings.Where(u => u.CheckInDate <= checkInDate.AddDays(i)
            && u.CheckOutDate > checkInDate.AddDays(i) && u.VillaId == villaId);

            foreach (var booking in villasBooked)
            {
                if (!bookingInDate.Contains(booking.Id))
                {
                    bookingInDate.Add(booking.Id);
                }
            }

            var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
            if (totalAvailableRooms == 0)
            {
                return 0;
            }
            else
            {
                if (finalAvailableRoomForAllNights > totalAvailableRooms)
                {
                    finalAvailableRoomForAllNights = totalAvailableRooms;
                }
            }
        }

        return finalAvailableRoomForAllNights;
    }

    public static RadialBarChartDTO GetRadialCartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
    {
        RadialBarChartDTO RadialBarChartDto = new();


        int increaseDecreaseRatio = 100;

        if (prevMonthCount != 0)
        {
            increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
        }

        RadialBarChartDto.TotalCount = totalCount;
        RadialBarChartDto.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
        RadialBarChartDto.HasRatioIncreased = currentMonthCount > prevMonthCount;
        RadialBarChartDto.Series = new int[] { increaseDecreaseRatio };

        return RadialBarChartDto;
    }
}
