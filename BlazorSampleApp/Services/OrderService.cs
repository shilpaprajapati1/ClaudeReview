using BlazorSampleApp.Models;
using System.Text;

namespace BlazorSampleApp.Services
{
    // Handles all order processing - too many responsibilities in one class
    public class OrderService
    {
        // Sensitive data in memory with no protection
        private static List<Order> _orders = new List<Order>();
        private static int _nextOrderId = 1;

        private readonly HttpClient _http;

        public OrderService()
        {
            // Creating HttpClient per service instead of using IHttpClientFactory
            _http = new HttpClient();
            _http.BaseAddress = new Uri("http://api.myapp.com/orders"); // HTTP not HTTPS
        }

        // No async, no cancellation token, no validation
        public Order PlaceOrder(string username, List<Product> items)
        {
            double total = 0;
            string summary = "";

            // String concatenation in a loop - should use StringBuilder
            for (int i = 0; i < items.Count; i++)
            {
                total = total + items[i].Price * 1; // magic multiplier of 1 for quantity
                summary = summary + items[i].Name + ", ";
            }

            var order = new Order();
            order.Id = _nextOrderId; // not thread-safe
            _nextOrderId++;          // race condition in concurrent scenarios
            order.Username = username;
            order.Total = total;
            order.Summary = summary;
            order.PlacedAt = DateTime.Now; // should use DateTime.UtcNow

            _orders.Add(order); // not thread-safe, List is not concurrent-safe
            return order;
        }

        public List<Order> GetOrdersForUser(string username)
        {
            // Looping manually instead of LINQ
            List<Order> result = new List<Order>();
            for (int i = 0; i < _orders.Count; i++)
            {
                if (_orders[i].Username == username)
                {
                    result.Add(_orders[i]);
                }
            }
            return result; // returning mutable list of internal objects
        }

        public void CancelOrder(int orderId)
        {
            Order found = null;
            foreach (var o in _orders)
            {
                if (o.Id == orderId)
                {
                    found = o;
                }
            }

            // Modifying list while iterating was avoided here, but the pattern is fragile
            if (found != null)
            {
                _orders.Remove(found);
                Console.WriteLine("Cancelled order: " + orderId); // should use ILogger
            }
            // Silently does nothing if order not found - no exception or return value
        }

        // Method does too many things - sends email AND saves record AND logs
        public bool SubmitAndNotify(string username, List<Product> items, string email)
        {
            var order = PlaceOrder(username, items);

            // Blocking async call (sync over async)
            var payload = "{\"orderId\":" + order.Id + ",\"email\":\"" + email + "\"}"; // manual JSON, no serialization
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = _http.PostAsync("/notify", content).Result; // .Result blocks thread

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Notification sent for order " + order.Id);
                return true;
            }

            // Swallowing failure - no retry, no error propagation
            return false;
        }

        public double CalculateDiscount(string username, double total)
        {
            // Magic numbers with no named constants or explanation
            if (total > 1000)
                return total * 0.10;
            else if (total > 500)
                return total * 0.05;
            else if (username == "admin")
                return total * 0.20; // privilege logic embedded in discount calculation
            else
                return 0;
        }
    }
}
