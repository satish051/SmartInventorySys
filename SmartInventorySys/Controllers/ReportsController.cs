using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;

namespace SmartInventorySys.Controllers
{
    [Authorize] // FIX: Allow ANY logged-in user to access this controller generally
    public class ReportsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportsController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // ---------------- ADMIN ONLY SECTION ----------------

        // GET: All Sales Report (Admin Only)
        [Authorize(Roles = "Admin")] // <--- Lock this specific page
        public async Task<IActionResult> SalesReport(DateTime? fromDate, DateTime? toDate, string searchString)
        {
            if (!fromDate.HasValue) fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!toDate.HasValue) toDate = DateTime.Now;

            var allOrders = await _unitOfWork.Order.GetAllAsync();

            // 1. Filter by Date
            var reportData = allOrders
                .Where(o => o.OrderDate.Date >= fromDate.Value.Date && o.OrderDate.Date <= toDate.Value.Date);

            // 2. Filter by Search (Invoice No)
            if (!string.IsNullOrEmpty(searchString))
            {
                reportData = reportData.Where(o => o.OrderNumber.ToLower().Contains(searchString.ToLower()));
            }

            var finalData = reportData.OrderByDescending(o => o.OrderDate).ToList();

            ViewData["FromDate"] = fromDate.Value.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate.Value.ToString("yyyy-MM-dd");
            ViewData["CurrentFilter"] = searchString; // Pass it back

            ViewData["TotalSales"] = finalData.Sum(x => x.TotalAmount);
            ViewData["TotalTax"] = finalData.Sum(x => x.TaxAmount);
            ViewData["TotalDiscount"] = finalData.Sum(x => x.DiscountAmount);

            return View(finalData);
        }

        // POST: Delete Order (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // <--- Lock this action
        public async Task<IActionResult> DeleteOrder(int id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.Id == id, includeProperties: "OrderDetails");
                if (order == null) return NotFound();

                // Restock
                foreach (var item in order.OrderDetails)
                {
                    var product = await _unitOfWork.Product.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _unitOfWork.Product.Update(product);
                    }
                }

                _unitOfWork.Order.Remove(order);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();

                return RedirectToAction(nameof(SalesReport));
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return RedirectToAction(nameof(SalesReport));
            }
        }

        // GET: Edit Order (Admin Only)
        [Authorize(Roles = "Admin")] // <--- Lock this
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _unitOfWork.Order.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Edit Order (Admin Only)
        [HttpPost]
        [Authorize(Roles = "Admin")] // <--- Lock this
        public async Task<IActionResult> EditOrder(int id, string comments)
        {
            var order = await _unitOfWork.Order.GetByIdAsync(id);
            if (order == null) return NotFound();

            order.Comments = comments;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(SalesReport));
        }

        // ---------------- STAFF / EVERYONE SECTION ----------------

        // GET: My Sales (Accessible to Staff, Manager, Admin)
        // No [Authorize(Roles="Admin")] here, so anyone logged in can see it.
        public async Task<IActionResult> MySales(DateTime? fromDate, DateTime? toDate, string searchString)
        {
            var userId = _userManager.GetUserId(User);

            if (!fromDate.HasValue) fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!toDate.HasValue) toDate = DateTime.Now;

            var allOrders = await _unitOfWork.Order.GetAllAsync();

            var myOrders = allOrders.Where(o => o.UserId == userId &&
                                    o.OrderDate.Date >= fromDate.Value.Date &&
                                    o.OrderDate.Date <= toDate.Value.Date);

            if (!string.IsNullOrEmpty(searchString))
            {
                myOrders = myOrders.Where(o => o.OrderNumber.ToLower().Contains(searchString.ToLower()));
            }

            var finalData = myOrders.OrderByDescending(o => o.OrderDate).ToList();

            ViewData["FromDate"] = fromDate.Value.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate.Value.ToString("yyyy-MM-dd");
            ViewData["CurrentFilter"] = searchString;
            ViewData["MyTotalSales"] = finalData.Sum(x => x.TotalAmount);

            return View(finalData);
        }

        // GET: Edit My Order (Staff can edit their OWN order)
        public async Task<IActionResult> EditMyOrder(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _unitOfWork.Order.GetByIdAsync(id);

            if (order == null) return NotFound();

            // SECURITY CHECK: Only allow if it's MY order OR I am an Admin
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Stop them if they try to hack the URL
            }

            return View(order);
        }

        // POST: Edit My Order (Save Changes)
        [HttpPost]
        public async Task<IActionResult> EditMyOrder(int id, string comments)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _unitOfWork.Order.GetByIdAsync(id);

            if (order == null) return NotFound();

            // SECURITY CHECK AGAIN
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Only allow editing the comment (Staff shouldn't change money amounts after sale!)
            order.Comments = comments;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(MySales));
        }

        // POST: Reports/DeleteMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMultiple(int[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
            {
                // Optional: Add TempData message "No items selected"
                return RedirectToAction(nameof(SalesReport));
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var id in selectedIds)
                {
                    var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.Id == id, includeProperties: "OrderDetails");
                    if (order != null)
                    {
                        // Restock Items
                        foreach (var item in order.OrderDetails)
                        {
                            var product = await _unitOfWork.Product.GetByIdAsync(item.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity += item.Quantity;
                                _unitOfWork.Product.Update(product);
                            }
                        }
                        // Delete Order
                        _unitOfWork.Order.Remove(order);
                    }
                }

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
            }

            return RedirectToAction(nameof(SalesReport));
        }

        // GET: Reports/PrintMultiple
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PrintMultiple(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return RedirectToAction(nameof(SalesReport));

            // Convert string "1,2,3" to list of ints
            var idList = ids.Split(',').Select(int.Parse).ToList();

            var orders = new List<Order>();
            foreach (var id in idList)
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.Id == id, includeProperties: "OrderDetails,OrderDetails.Product");
                if (order != null) orders.Add(order);
            }

            return View(orders);
        }
    }
}