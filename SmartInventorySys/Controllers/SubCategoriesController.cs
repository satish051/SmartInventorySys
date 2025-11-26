using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;

namespace SmartInventorySys.Controllers
{
    [Authorize]
    public class SubCategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubCategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: SubCategories
        public async Task<IActionResult> Index(string searchString)
        {
            var subCategories = await _unitOfWork.SubCategory.GetAllAsync(includeProperties: "Category");

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                subCategories = subCategories.Where(s => s.Name.ToLower().Contains(searchString)
                                                      || (s.Category != null && s.Category.Name.ToLower().Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(subCategories);
        }

        // GET: SubCategories/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: SubCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategory subCategory)
        {
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _unitOfWork.SubCategory.Add(subCategory);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", subCategory.CategoryId);
            return View(subCategory);
        }

        // GET: SubCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subCategory = await _unitOfWork.SubCategory.GetByIdAsync(id.Value);
            if (subCategory == null) return NotFound();

            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", subCategory.CategoryId);
            return View(subCategory);
        }

        // POST: SubCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategory subCategory)
        {
            if (id != subCategory.Id) return NotFound();

            // FIX: Ignore the full Category object validation
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _unitOfWork.SubCategory.Update(subCategory);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload list if validation fails
            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", subCategory.CategoryId);

            return View(subCategory);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategory = await _unitOfWork.SubCategory.GetByIdAsync(id);
            if (subCategory != null)
            {
                _unitOfWork.SubCategory.Remove(subCategory);
                await _unitOfWork.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}