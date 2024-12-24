using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Data;
using CapstoneProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneProject.Controllers;
[Authorize(Policy ="Inventory")]
public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }


    public async Task<IActionResult> Index()
    {
        var categories = await _context.categories.ToListAsync();

        ViewBag.ErrorMessage = TempData["Error"];
        return View(categories);
    }

    public IActionResult AddCategory()
    {
        return View("Add");
    }


    [HttpPost]
    public async Task<IActionResult> AddCategory(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View("Add", category);
    }

    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View("Edit", category);
    }

    [HttpPost]
    public async Task<IActionResult> EditCategory(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View("Edit", category);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }
        if (category.Products.Any())
        {
            TempData["Error"] = "Cann't delete categories, as it was taken by one or more products.";
            return RedirectToAction(nameof(Index));
        }
        _context.categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ProductsByCategory(int id)
    {
        var category = await _context.categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        ViewBag.CategoryName = category.Name;
        return View(category.Products);
    }
}
