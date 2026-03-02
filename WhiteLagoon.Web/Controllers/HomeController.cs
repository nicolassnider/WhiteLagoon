using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()

        {
            HomeVM homeVM = new HomeVM()
            {
                Villas = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity").ToList(),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now)
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villasList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(u => u.Status == SD.Status_Approved ||
            u.Status == SD.Status_CheckedIn).ToList();
            foreach (var villa in villasList)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumberList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }

            HomeVM homeVM = new HomeVM()
            {
                CheckInDate = checkInDate,
                Villas = villasList,
                Nights = nights
            };
            return PartialView("_VillaListPartial", homeVM);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
