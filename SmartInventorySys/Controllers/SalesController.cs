using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace SmartInventorySys.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public SalesController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager; // Initialize
        }

        // GET: Show POS
        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.Product.GetAllAsync();
            return View(products);
        }

        // POST: Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            // FIX: Check if the request itself is null (This prevents the crash)
            if (request == null)
            {
                return Json(new { success = false, message = "Invalid data received. Request is null." });
            }

            if (request.CartItems == null || request.CartItems.Count == 0)
            {
                return Json(new { success = false, message = "Cart is empty!" });
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {

                // 1. Get Current User Details
                var user = await _userManager.GetUserAsync(User);
                string salesPerson = user?.UserName ?? "Unknown Staff";


                // 2. Format the Comment: "User's Note | Sold by: Username"
                string finalComment = string.IsNullOrWhiteSpace(request.Comments)
                    ? $"Sold by: {salesPerson}"
                    : $"{request.Comments} (Sold by: {salesPerson})";



                // ... (Rest of the logic remains exactly the same) ...
                var order = new Order
                {
                    OrderNumber = "INV-" + DateTime.Now.Ticks.ToString(),
                    OrderDate = DateTime.Now,

                    // FIX: Save the formatted comment with the username
                    Comments = finalComment,

                    UserId = user?.Id, // We still save the ID for filtering, but the name is now in the text too
                    SubTotal = 0,
                    DiscountAmount = 0,
                    TaxAmount = 0,
                    TotalAmount = 0
                };

                _unitOfWork.Order.Add(order);
                await _unitOfWork.SaveAsync();

                decimal subTotal = 0;

                foreach (var item in request.CartItems)
                {
                    var product = await _unitOfWork.Product.GetByIdAsync(item.ProductId);

                    if (product == null) continue;

                    if (product.StockQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Json(new { success = false, message = $"Not enough stock for {product.Name}!" });
                    }

                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };
                    _unitOfWork.OrderDetail.Add(detail);

                    product.StockQuantity -= item.Quantity;
                    _unitOfWork.Product.Update(product);

                    subTotal += (product.Price * item.Quantity);
                }

                // Calculations
                decimal discountAmount = (subTotal * request.DiscountPercent) / 100;
                decimal taxableAmount = subTotal - discountAmount;
                decimal taxAmount = taxableAmount * 0.13m;

                order.SubTotal = subTotal;
                order.DiscountAmount = discountAmount;
                order.TaxAmount = taxAmount;
                order.TotalAmount = taxableAmount + taxAmount;

                _unitOfWork.Order.Update(order);
                await _unitOfWork.CommitTransactionAsync();

                return Json(new
                {
                    success = true,
                    message = "Sale successful!",
                    id = order.Id,
                    invoiceId = order.OrderNumber,
                    totalAmount = order.TotalAmount
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }   





        // GET: Invoice
        public async Task<IActionResult> Invoice(int? id)
        {
            if (id == null) return NotFound();

            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                o => o.Id == id,
                includeProperties: "OrderDetails,OrderDetails.Product"
            );

            if (order == null) return NotFound();

            return View(order);
        }

        // ---------------- KHALTI PAYMENT ----------------
        [HttpPost]
        public async Task<IActionResult> PayWithKhalti([FromBody] int orderId)
        {
            var order = await _unitOfWork.Order.GetByIdAsync(orderId);
            if (order == null) return Json(new { success = false, message = "Order not found" });

            // Fix: Use the variable secretKey properly
            var secretKey = "Key live_secret_key_68791341fdd94846a146f0457ff7b455";

            var payload = new
            {
                return_url = "https://localhost:7208/Sales/KhaltiCallback", // Ensure port matches!
                website_url = "https://localhost:7208",
                amount = (int)(order.TotalAmount * 100),
                purchase_order_id = order.OrderNumber,
                purchase_order_name = "Store Purchase " + order.OrderNumber
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", secretKey); // Fix: Use variable

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://a.khalti.com/api/v2/epayment/initiate/", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var khaltiResp = JsonConvert.DeserializeObject<KhaltiResponse>(responseString);
                    // Fix: Check for null response
                    if (khaltiResp != null)
                    {
                        return Json(new { success = true, paymentUrl = khaltiResp.payment_url });
                    }
                }

                return Json(new { success = false, message = "Khalti Error: " + responseString });
            }
        }

        [HttpGet]
        public async Task<IActionResult> KhaltiCallback(string pidx, string transaction_id, string amount, string purchase_order_id, string status)
        {
            if (status == "Completed")
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderNumber == purchase_order_id);

                if (order != null)
                {
                    order.PaymentMethod = "Khalti";
                    _unitOfWork.Order.Update(order);
                    await _unitOfWork.SaveAsync();

                    return RedirectToAction("Invoice", new { id = order.Id });
                }
            }

            return Content("Payment Failed or Verification Error");
        }

        // ---------------- ESEWA PAYMENT ----------------
        [HttpGet]
        public async Task<IActionResult> EsewaCallback(string oid, string amt, string refId)
        {
            var verifyUrl = "https://uat.esewa.com.np/epay/transrec";
            var scd = "EPAYTEST";

            var values = new Dictionary<string, string>
            {
                { "amt", amt },
                { "scd", scd },
                { "rid", refId },
                { "pid", oid }
            };

            var content = new FormUrlEncodedContent(values);

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(verifyUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (responseString.Contains("Success"))
                {
                    var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderNumber == oid);

                    if (order != null)
                    {
                        order.PaymentMethod = "eSewa";
                        _unitOfWork.Order.Update(order);
                        await _unitOfWork.SaveAsync();

                        return RedirectToAction("Invoice", new { id = order.Id });
                    }
                }
            }

            return Content($"eSewa Verification Failed! Response: {oid}");
        }

        // Fix: Removed 'async' and 'Task' because there is no await inside
        [HttpGet]
        public IActionResult EsewaFailure()
        {
            return Content("Payment Failed or Cancelled by User.");
        }
    }
}