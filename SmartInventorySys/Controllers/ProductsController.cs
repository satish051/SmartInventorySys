using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;
using System.IO;

namespace SmartInventorySys.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? subCategoryId)
        {
            // 1. Fetch ALL products with related data
            var products = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category,SubCategory");

            // 2. Filter by Name/SKU
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchString.ToLower())
                                            || p.SKU.ToLower().Contains(searchString.ToLower()));
            }

            // 3. Filter by Category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // 4. Filter by Sub-Category
            if (subCategoryId.HasValue && subCategoryId.Value > 0)
            {
                products = products.Where(p => p.SubCategoryId == subCategoryId.Value);
            }

            // 5. Load Dropdown Lists for the View
            var categories = await _unitOfWork.Category.GetAllAsync();
            var subCategories = await _unitOfWork.SubCategory.GetAllAsync();

            ViewData["Categories"] = new SelectList(categories, "Id", "Name", categoryId);

            // Improved: Only show relevant sub-categories if a category is selected
            if (categoryId.HasValue)
            {
                subCategories = subCategories.Where(s => s.CategoryId == categoryId.Value);
            }
            ViewData["SubCategories"] = new SelectList(subCategories, "Id", "Name", subCategoryId);

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentSubCategory"] = subCategoryId;

            return View(products);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");

            // NEW: Load SubCategories
            var subCategories = await _unitOfWork.SubCategory.GetAllAsync();
            ViewData["SubCategoryId"] = new SelectList(subCategories, "Id", "Name");

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SKU,Price,StockQuantity,LowStockThreshold,ImageUrl,CategoryId,SubCategoryId")] Product product)
        {
            ModelState.Remove("Category");
            ModelState.Remove("SubCategory"); // Ignore validation for the object

            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(product);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload lists if it fails
            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", product.CategoryId);

            var subCategories = await _unitOfWork.SubCategory.GetAllAsync();
            ViewData["SubCategoryId"] = new SelectList(subCategories, "Id", "Name", product.SubCategoryId);

            return View(product);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _unitOfWork.Product.GetByIdAsync(id.Value);
            if (product == null) return NotFound();

            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SKU,Price,StockQuantity,LowStockThreshold,ImageUrl,CategoryId")] Product product)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _unitOfWork.Product.GetByIdAsync(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unitOfWork.Product.GetByIdAsync(id);
            if (product != null)
            {
                _unitOfWork.Product.Remove(product);
                await _unitOfWork.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }

       
    }
}