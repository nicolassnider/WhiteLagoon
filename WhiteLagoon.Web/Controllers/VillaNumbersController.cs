using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

public class VillaNumbersController : Controller
{
    private readonly ApplicationDbContext _context;

    public VillaNumbersController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        var villaNumbers = _context.VillaNumbers
            .Include(vn => vn.Villa)
            .ToList();
        return View(villaNumbers);
    }

    public IActionResult Create()
    {
        VillaNumberVM villaNumberVM = new()
        {
            VillaList = _context.Villas
            .ToList()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            })
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

        bool roomNumberExists = _context.VillaNumbers.Any(
            u => u.VillaId == villaNumberVM.VillaNumber.VillaId &&
            u.Villa_Number == villaNumberVM.VillaNumber.Villa_Number
            );

        if (ModelState.IsValid && !roomNumberExists)
        {
            _context.VillaNumbers.Add(villaNumberVM.VillaNumber);
            _context.SaveChanges();

            TempData["success"] = "Villa Number created successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "Villa Number not created successfully.";

        villaNumberVM.VillaList = _context.Villas
            .ToList()
            .Select(u => new SelectListItem
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
            VillaList = _context.Villas
            .ToList()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            VillaNumber = _context.VillaNumbers.FirstOrDefault(vn => vn.Villa_Number == villaNumberId)
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
            _context.VillaNumbers.Update(villaNumberVM.VillaNumber);
            _context.SaveChanges();

            TempData["success"] = "Villa Number updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "Villa Number not created successfully.";

        villaNumberVM.VillaList = _context.Villas
            .ToList()
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
            VillaList = _context.Villas
            .ToList()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            VillaNumber = _context.VillaNumbers.FirstOrDefault(vn => vn.Villa_Number == villaNumberId)
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
        _context.VillaNumbers.Remove(villaNumberVM.VillaNumber);
        _context.SaveChanges();
        TempData["success"] = "Villa Number deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
