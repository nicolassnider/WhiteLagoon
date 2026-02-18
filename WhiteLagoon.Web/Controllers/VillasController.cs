using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers;

public class VillasController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public VillasController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index() // GET: Villas, url: /Villas
    {
        var villas = _unitOfWork.VillaRepository.GetAll();

        return View(villas); // we must return villlas to the view, otherwise we will get an error because the view is expecting a model of type List<Villa>
    }

    public IActionResult Create() // GET: Villas/Create, url: /Villas/Create
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Villa villa) // POST: Villas/Create, url: /Villas/Create
    {

        if (villa.Name == villa.Description)
        {
            ModelState.AddModelError("", "The DisplayOrder cannot exactly match the Name.");
        }
        if (ModelState.IsValid)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images/VillaImages");

                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                    villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\images\VillaImages\" + fileName;

            }
            else
            {
                villa.ImageUrl = "https:\\placehold.co/600x400";
            }
            _unitOfWork.VillaRepository.Add(villa);
            _unitOfWork.Save();
            TempData["success"] = "Villa created successfully.";
            return RedirectToAction(nameof(Index), "Villas");
        }
        else
        {
            TempData["error"] = "Villa not created successfully.";
        }

        return View(villa); // we must return villlas to the view, otherwise we will get an error because the view is expecting a model of type Villa

    }

    public IActionResult Edit(int villaId) // GET: Villas/Edit/5, url: /Villas/Edit/5
    {
        var villa = _unitOfWork.VillaRepository.Get(u => u.Id == villaId);
        return View(villa);
    }

    [HttpPost]
    public IActionResult Update(Villa villa) // POST: Villas/Create, url: /Villas/Create
    {

        if (villa.Name == villa.Description)
        {
            ModelState.AddModelError("", "The DisplayOrder cannot exactly match the Name.");

        }
        if (ModelState.IsValid)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images/VillaImages");

                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                    villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\images\VillaImages\" + fileName;

            }
            else
            {
                villa.ImageUrl = "https:\\placehold.co/600x400";
            }
            _unitOfWork.VillaRepository.Update(villa);
            _unitOfWork.Save();
            TempData["success"] = "Villa updated successfully.";
            return RedirectToAction(nameof(Index), "Villas");
        }
        else { TempData["error"] = "Villa not updated successfully."; }

        return View(villa); // we must return villlas to the view, otherwise we will get an error because the view is expecting a model of type Villa

    }

    public IActionResult Delete(int villaId) // GET: Villas/Delete/5, url: /Villas/Delete/5
    {
        Villa? villa = _unitOfWork.VillaRepository.Get(v => v.Id == villaId);
        if (villa == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(villa);
    }

    [HttpPost]
    public IActionResult Delete(Villa villa) // POST: Villas/Delete, url: /Villas/Delete
    {

        Villa? dbVilla = _unitOfWork.VillaRepository.Get(v => v.Id == villa.Id);
        if (dbVilla != null)
        {

            if (!string.IsNullOrEmpty(villa.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _unitOfWork.VillaRepository.Remove(dbVilla);
            _unitOfWork.Save();
            TempData["success"] = "Villa deleted successfully.";
            return RedirectToAction(nameof(Index), "Villas");
        }
        else { TempData["error"] = "Villa not deleted successfully."; }

        return View(dbVilla);

    }
}
