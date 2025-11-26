using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore; // Needed for concurrency exceptions if any
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;

namespace SmartInventorySys.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork; // DI: Using Interface

        public SuppliersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index(string searchString)
        {
            var suppliers = await _unitOfWork.Supplier.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                suppliers = suppliers.Where(s => s.Name.ToLower().Contains(searchString)
                                              || s.ContactPerson.ToLower().Contains(searchString)
                                              || s.Phone.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(suppliers);
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ContactPerson,Email,Phone,Address")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Supplier.Add(supplier);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _unitOfWork.Supplier.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactPerson,Email,Phone,Address")] Supplier supplier)
        {
            if (id != supplier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Supplier.Update(supplier);
                    await _unitOfWork.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _unitOfWork.Supplier.GetByIdAsync(supplier.Id);
                    if (exists == null) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _unitOfWork.Supplier.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _unitOfWork.Supplier.GetByIdAsync(id);
            if (supplier != null)
            {
                _unitOfWork.Supplier.Remove(supplier);
                await _unitOfWork.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}