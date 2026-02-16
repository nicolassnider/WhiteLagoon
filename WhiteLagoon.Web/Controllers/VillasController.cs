using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers;

public class VillasController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    public VillasController(ApplicationDbContext applicationDbContext)
    {
        _dbContext = applicationDbContext;
    }
    public IActionResult Index() // GET: Villas, url: /Villas
    {
        var villas = _dbContext.Villas.ToList();

        return View(villas); // we must return villlas to the view, otherwise we will get an error because the view is expecting a model of type List<Villa>
    }

    public IActionResult Create() // GET: Villas/Create, url: /Villas/Create
    {
        return View();
    }

    public IActionResult Edit(int id) // GET: Villas/Edit/5, url: /Villas/Edit/5
    {
        var villa = _dbContext.Villas.Find(id);
        return View(villa);
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
            _dbContext.Villas.Add(villa);
            _dbContext.SaveChanges();
            return RedirectToAction("Index", "Villas");
        }

        return View(villa); // we must return villlas to the view, otherwise we will get an error because the view is expecting a model of type Villa

    }

    public IActionResult Update(int villaId) // GET: Villas/Update/5, url: /Villas/Update/5
    {
        Villa? villa = _dbContext.Villas.FirstOrDefault(villa => villa.Id == villaId);

        /*other examples
         * 
         * Villa? villa = _dbContext.Villas.Find(villaId);
         * Villa? villa = _dbContext.Villas.Where(villa => villa.Id == villaId).FirstOrDefault() && villa.occupancy>0;
         */
        if (villa == null)
        {
            RedirectToAction("Error", "Home"); // redirect to error page
        }
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
            _dbContext.Villas.Update(villa);
            _dbContext.SaveChanges();
            return RedirectToAction("Index", "Villas");
        }

        return View(villa); // we must return villlas to the view, otherwise we will get an error because the view is expecting a model of type Villa

    }

    public IActionResult Delete(int villaId) // GET: Villas/Delete/5, url: /Villas/Delete/5
    {
        Villa? villa = _dbContext.Villas.Find(villaId);
        if (villa == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(villa);
    }

    [HttpPost]
    public IActionResult Delete(Villa villa) // POST: Villas/Delete, url: /Villas/Delete
    {

        Villa? dbVilla = _dbContext.Villas.Find(villa.Id);
        if (dbVilla != null)
        {
            _dbContext.Villas.Remove(dbVilla);
            _dbContext.SaveChanges();
            return RedirectToAction("Index", "Villas");
        }

        return View(dbVilla);

    }
}
