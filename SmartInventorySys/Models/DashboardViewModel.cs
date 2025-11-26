namespace SmartInventorySys.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        // Data for the Chart (X and Y axis)
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> ChartValues { get; set; } = new List<decimal>();
    }
}