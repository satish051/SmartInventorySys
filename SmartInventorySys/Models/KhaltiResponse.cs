namespace SmartInventorySys.Models
{
    public class KhaltiResponse
    {
        public string pidx { get; set; } = string.Empty;
        public string payment_url { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public int expires_at { get; set; }
    }
}