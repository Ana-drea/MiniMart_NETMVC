using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Data;
using CapstoneProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Controllers
{
    [Authorize(Policy = "Inventory")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> ProductsByCategory(int id)
        {
            var products = await _context.products
                                        .Include(p => p.Category)
                                        .Where(p => p.CategoryId == id)
                                        .ToListAsync();

            if (!products.Any()) // Simplify checking for empty or null list
            {
                ViewBag.CategoryName = _context.categories.Where(c => c.Id == id).Select(c => c.Name).FirstOrDefault() ?? "Unknown Category";
                return View("Index", new List<Product>());
            }

            ViewBag.CategoryName = products.First().Category?.Name;
            return View("Index", products);
        }

        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(_context.categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (!_context.categories.Any(c => c.Id == product.CategoryId))
                {
                    ModelState.AddModelError("CategoryId", "The selected category does not exist.");
                    ViewBag.Categories = new SelectList(_context.categories, "Id", "Name");
                    return View(product);
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var imagePath = Path.Combine("wwwroot/images", fileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = $"/images/{fileName}";
                }
                else
                {
                    product.ImageUrl = null;
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.categories, "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                if (!_context.categories.Any(c => c.Id == product.CategoryId))
                {
                    ModelState.AddModelError("CategoryId", "The selected category does not exist.");
                    ViewBag.Categories = new SelectList(_context.categories, "Id", "Name", product.CategoryId);
                    return View(product);
                }

                try
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.PurchasePrice = product.PurchasePrice;
                    existingProduct.Price = product.Price;
                    existingProduct.QuantityInStock = product.QuantityInStock;
                    existingProduct.CategoryId = product.CategoryId;
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var imagePath = Path.Combine("wwwroot/images", fileName);

                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        existingProduct.ImageUrl = $"/images/{fileName}";
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error updating product with ID {product.Id}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }

                    ModelState.AddModelError("", "Unable to save changes. Please try again, and if the problem persists, contact support.");
                }
            }

            ViewBag.Categories = new SelectList(_context.categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.products.FindAsync(id);
            if (product != null)
            {
                _context.products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
