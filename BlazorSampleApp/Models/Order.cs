namespace BlazorSampleApp.Models
{
    // No validation attributes, no encapsulation
    public class Order
    {
        public int Id;
        public string Username;
        public double Total;       // should be decimal for currency
        public string Summary;
        public DateTime PlacedAt;
        public bool IsCancelled;   // unused field left in model
        public string status = "pending"; // public field, lowercase - inconsistent style
    }
}
