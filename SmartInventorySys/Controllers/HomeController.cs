using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartInventorySys.Data;
using SmartInventorySys.Models;
using System.Diagnostics;
using Newtonsoft.Json; // Make sure this is included for the chart data

namespace SmartInventorySys.Controllers
{
    [Authorize] // Require login to see the dashboard
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Fetch Basic Stats
            var dashboard = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity <= p.LowStockThreshold),
                TotalOrders = await _context.Orders.CountAsync(),

                // Handle null sum if no orders exist yet
                TotalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0
            };

            // 2. Prepare Data for the Chart (Last 7 Days of Sales)
            // We group orders by Date and Sum the TotalAmount
            var salesData = await _context.Orders
                .Where(o => o.OrderDate >= DateTime.Today.AddDays(-7))
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // 3. Fill the Lists for Chart.js
            foreach (var item in salesData)
            {
                dashboard.ChartLabels.Add(item.Date.ToString("MMM dd")); // e.g., "Nov 24"
                dashboard.ChartValues.Add(item.Total);
            }

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}