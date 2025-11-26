using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;

namespace SmartInventorySys.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string searchString)
        {
            var categories = await _unitOfWork.Category.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name.ToLower().Contains(searchString.ToLower()));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            // Fix: Remove validation for "Products" list
            ModelState.Remove("Products");

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _unitOfWork.Category.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id) return NotFound();

            ModelState.Remove("Products");

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _unitOfWork.Category.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _unitOfWork.Category.GetByIdAsync(id);
            if (category != null)
            {
                _unitOfWork.Category.Remove(category);
                await _unitOfWork.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}