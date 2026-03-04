

using WhiteLagoon.Application.DTOs;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<RadialBarChartDTO> GetTotalBookingRadialChartData();
    Task<RadialBarChartDTO> GetRegisteredUserChartData();
    Task<RadialBarChartDTO> GetRevenueChartData();
    Task<PieChartDTO> GetBookingPieChartData();
    Task<LineChartDTO> GetMemberAndBookingLineChartData();
}
