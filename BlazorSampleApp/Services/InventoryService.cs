using BlazorSampleApp.Models;

namespace BlazorSampleApp.Services
{
    // Manages inventory levels and stock alerts
    public class InventoryService
    {
        // Static mutable state - not thread-safe
        private static List<Product> _inventory = new List<Product>();
        private static string _lastAuditUser = "";

        private readonly HttpClient _http;

        public InventoryService()
        {
            // Should use IHttpClientFactory
            _http = new HttpClient();
            _http.BaseAddress = new Uri("http://inventory.myapp.com/api"); // HTTP not HTTPS
        }

        // Returns internal list directly - caller can mutate it
        public List<Product> GetAllStock()
        {
            return _inventory;
        }

        public void AddStock(Product p, int quantity)
        {
            // No null check on p, no validation on quantity
            p.Stock = p.Stock + quantity;
            _inventory.Add(p); // duplicate entries possible, no check
            Console.WriteLine("Stock added for: " + p.Name + " qty: " + quantity); // should use ILogger
        }

        public bool DeductStock(int productId, int quantity)
        {
            Product found = null;
            // Manual loop instead of LINQ
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i].Id == productId)
                {
                    found = _inventory[i];
                }
            }

            if (found == null)
                return false;

            // No thread-safety on this mutation
            found.Stock = found.Stock - quantity;
            return true;
        }

        public double GetTotalInventoryValue()
        {
            // double used for currency calculation - should be decimal
            double total = 0;
            foreach (var p in _inventory)
            {
                // Magic multiplier, string concat in loop for debug
                total = total + (p.Price * p.Stock);
                Console.WriteLine("Processing: " + p.Name + " value: " + (p.Price * p.Stock));
            }
            return total;
        }

        public Product GetProduct(int id)
        {
            // Direct dictionary/list access without null guard
            return _inventory.First(p => p.Id == id); // throws InvalidOperationException if not found
        }

        public void AuditStock(string adminUser, string password)
        {
            // Hardcoded credential check, plaintext password
            if (adminUser == "admin" && password == "admin123")
            {
                _lastAuditUser = adminUser;
                // Blocking async call
                var response = _http.GetAsync("/audit/trigger").Result;
                Console.WriteLine("Audit triggered by: " + adminUser);
            }
            // Silently does nothing if credentials are wrong - no feedback
        }

        public string GenerateStockReport()
        {
            // String concatenation in a loop - should use StringBuilder
            string report = "Stock Report\n";
            report = report + "============\n";
            foreach (var p in _inventory)
            {
                report = report + p.Name + ": " + p.Stock + " units @ $" + p.Price + "\n";
            }
            report = report + "Total Value: $" + GetTotalInventoryValue();
            return report;
        }

        public async Task<bool> SyncWithWarehouseAsync()
        {
            // No CancellationToken parameter
            var response = await _http.GetAsync("/sync");
            if (!response.IsSuccessStatusCode)
            {
                // Swallowing failure silently
                return false;
            }

            // TODO: parse response and update _inventory
            return true;
        }
    }
}
