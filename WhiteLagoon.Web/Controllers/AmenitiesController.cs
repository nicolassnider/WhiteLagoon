using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

[Authorize(Roles = SD.Role_Admin)]
public class AmenitiesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AmenitiesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index() // GET: Amenities, url: /Amenities
    {
        var amenities = _unitOfWork.AmenityRepository.GetAll(includeProperties: "Villa");

        return View(amenities); // 
    }

    public IActionResult Create()
    {
        AmenityVM amenityVM = new()
        {
            VillaList = _unitOfWork.VillaRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
        };
        return View(amenityVM);
    }

    [HttpPost]
    public IActionResult Create(AmenityVM amenityVm)
    {
        /*
         * Removing the key tells MVC to ignore that missing navigation value so the POST can succeed.
         */
        //ModelState.Remove("Villa");

        if (ModelState.IsValid)
        {
            _unitOfWork.AmenityRepository.Add(amenityVm.Amenity);
            _unitOfWork.Save();

            TempData["success"] = "Amenity created successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "Amenity not created successfully.";

        amenityVm.VillaList = _unitOfWork.VillaRepository
            .GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });


        return View(amenityVm);


    }

    public IActionResult Update(int amenityId)
    {
        AmenityVM amenityVM = new()
        {
            VillaList = _unitOfWork.VillaRepository
            .GetAll()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            Amenity = _unitOfWork.AmenityRepository.Get(vm => vm.Id == amenityId)
        };

        if (amenityVM.Amenity == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(amenityVM);
    }

    [HttpPost]
    public IActionResult Update(AmenityVM amenityVM)
    {
        /*
         * Removing the key tells MVC to ignore that missing navigation value so the POST can succeed.
         */
        //ModelState.Remove("Villa");

        if (ModelState.IsValid)
        {
            _unitOfWork.AmenityRepository.Update(amenityVM.Amenity);
            _unitOfWork.Save();

            TempData["success"] = "Amenity updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "Amenity not created successfully.";

        amenityVM.VillaList = _unitOfWork.VillaRepository.GetAll()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });


        return View(amenityVM);


    }

    public IActionResult Delete(int amenityId) // GET: Villas/Delete/5, url: /Villas/Delete/5
    {

        Amenity? amenity = _unitOfWork.AmenityRepository.Get(a => a.Id == amenityId, includeProperties: "Villa");
        if (amenity == null)
        {
            return RedirectToAction("Error", "Home");
        }
        AmenityVM amenityVM = new() { Amenity = amenity };
        return View(amenityVM);
    }

    [HttpPost]
    public IActionResult Delete(Amenity amenity) // POST: Villas/Delete, url: /Villas/Delete
    {

        Amenity? dbAmenity = _unitOfWork.AmenityRepository.Get(a => a.Id == amenity.Id);
        if (dbAmenity != null)
        {

            _unitOfWork.AmenityRepository.Remove(dbAmenity);
            _unitOfWork.Save();
            TempData["success"] = "Amenity deleted successfully.";
            return RedirectToAction(nameof(Index), "Amenities");
        }
        else { TempData["error"] = "Amenity not deleted successfully."; }

        return View(dbAmenity);

    }
}
