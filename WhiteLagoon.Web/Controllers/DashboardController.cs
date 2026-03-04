using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interfaces;
namespace WhiteLagoon.Web.Controllers
{
    public class DashboardController(IDashboardService dashboardService) : Controller
    {
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            return Json(await dashboardService.GetTotalBookingRadialChartData());

        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            return Json(await dashboardService.GetRegisteredUserChartData());

        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            return Json(await dashboardService.GetRevenueChartData());
        }

        public async Task<IActionResult> GetBookingPieChartData()
        {
            return Json(await dashboardService.GetBookingPieChartData());
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await dashboardService.GetMemberAndBookingLineChartData());
        }


    }
}