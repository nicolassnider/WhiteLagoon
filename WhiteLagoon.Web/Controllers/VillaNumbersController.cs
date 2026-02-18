using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

public class VillaNumbersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public VillaNumbersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        var villaNumbers = _unitOfWork.VillaNumberRepository.GetAll(includeProperties: "Villa");
        return View(villaNumbers);
    }

    public IActionResult Create()
    {
        VillaNumberVM villaNumberVM = new()
        {
            VillaList = _unitOfWork.VillaNumberRepository.GetAll(includeProperties: "Villa").Select(u => new SelectListItem
            {
                Text = u.Villa.Name,
                Value = u.VillaId.ToString()
            }),
        };
        return View(villaNumberVM);
    }

    [HttpPost]
    public IActionResult Create(VillaNumberVM villaNumberVM)
    {
        /*
         * Removing the key tells MVC to ignore that missing navigation value so the POST can succeed.
         */
        //ModelState.Remove("Villa");

        bool roomNumberExists = _unitOfWork.VillaNumberRepository.Get(u => u.Villa_Number == villaNumberVM.VillaNumber.Villa_Number) != null;

        if (ModelState.IsValid && !roomNumberExists)
        {
            _unitOfWork.VillaNumberRepository.Add(villaNumberVM.VillaNumber);
            _unitOfWork.Save();

            TempData["success"] = "Villa Number created successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "Villa Number not created successfully.";

        villaNumberVM.VillaList = _unitOfWork.VillaRepository
            .GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });


        return View(villaNumberVM);


    }

    public IActionResult Update(int villaNumberId)
    {
        VillaNumberVM villaNumberVM = new()
        {
            VillaList = _unitOfWork.VillaRepository
            .GetAll()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            VillaNumber = _unitOfWork.VillaNumberRepository.Get(vn => vn.Villa_Number == villaNumberId)
        };

        if (villaNumberVM.VillaNumber == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(villaNumberVM);
    }

    [HttpPost]
    public IActionResult Update(VillaNumberVM villaNumberVM)
    {
        /*
         * Removing the key tells MVC to ignore that missing navigation value so the POST can succeed.
         */
        //ModelState.Remove("Villa");

        if (ModelState.IsValid)
        {
            _unitOfWork.VillaNumberRepository.Update(villaNumberVM.VillaNumber);
            _unitOfWork.Save();

            TempData["success"] = "Villa Number updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "Villa Number not created successfully.";

        villaNumberVM.VillaList = _unitOfWork.VillaRepository.GetAll()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });


        return View(villaNumberVM);


    }

    public IActionResult Delete(int villaNumberId)
    {
        VillaNumberVM villaNumberVM = new()
        {
            VillaList = _unitOfWork.VillaRepository
            .GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            VillaNumber = _unitOfWork.VillaNumberRepository.Get(vn => vn.Villa_Number == villaNumberId)
        };

        if (villaNumberVM.VillaNumber == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(villaNumberVM);
    }

    [HttpPost]
    public IActionResult Delete(VillaNumberVM villaNumberVM)
    {
        if (villaNumberVM.VillaNumber == null)
        {
            TempData["error"] = "Villa Number not deleted successfully.";
            return RedirectToAction("Error", "Home");
        }
        _unitOfWork.VillaNumberRepository.Remove(villaNumberVM.VillaNumber);
        _unitOfWork.Save();
        TempData["success"] = "Villa Number deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
