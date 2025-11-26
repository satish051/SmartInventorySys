using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;

namespace SmartInventorySys.Controllers
{
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchasesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Purchases
        public async Task<IActionResult> Index(string searchString)
        {
            var purchases = await _unitOfWork.Purchase.GetAllAsync(includeProperties: "Product,Supplier");

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                purchases = purchases.Where(p => p.InvoiceNo.ToLower().Contains(searchString)
                                              || (p.Supplier != null && p.Supplier.Name.ToLower().Contains(searchString))
                                              || (p.Product != null && p.Product.Name.ToLower().Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(purchases);
        }

        // GET: Purchases/Create
        public async Task<IActionResult> Create()
        {
            // Fetch lists via UnitOfWork
            var products = await _unitOfWork.Product.GetAllAsync();
            var suppliers = await _unitOfWork.Supplier.GetAllAsync();

            ViewData["ProductId"] = new SelectList(products, "Id", "Name");
            ViewData["SupplierId"] = new SelectList(suppliers, "Id", "Name");
            return View();
        }

        // POST: Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PurchaseDate,InvoiceNo,SupplierId,ProductId,Quantity,UnitCost")] Purchase purchase)
        {
            // 1. Logic
            purchase.TotalCost = purchase.Quantity * purchase.UnitCost;

            ModelState.Remove("Supplier");
            ModelState.Remove("Product");

            if (ModelState.IsValid)
            {
                // 2. Add Purchase
                _unitOfWork.Purchase.Add(purchase);

                // 3. Update Stock using Repository
                var product = await _unitOfWork.Product.GetByIdAsync(purchase.ProductId);
                if (product != null)
                {
                    product.StockQuantity += purchase.Quantity;
                    _unitOfWork.Product.Update(product); // Explicit Update
                }

                // 4. Commit all changes
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload lists if validation fails
            var products = await _unitOfWork.Product.GetAllAsync();
            var suppliers = await _unitOfWork.Supplier.GetAllAsync();

            ViewData["ProductId"] = new SelectList(products, "Id", "Name", purchase.ProductId);
            ViewData["SupplierId"] = new SelectList(suppliers, "Id", "Name", purchase.SupplierId);
            return View(purchase);
        }
    }
}