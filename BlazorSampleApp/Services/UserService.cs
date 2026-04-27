using BlazorSampleApp.Models;

namespace BlazorSampleApp.Services
{
    // Singleton service managing user sessions and product data
    public class UserService
    {
        private static List<string> _loggedInUsers = new List<string>();
        private static Dictionary<int, Product> _productCache = new Dictionary<int, Product>();

        private readonly HttpClient _http;

        public UserService()
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri("http://api.myapp.com"); // hardcoded, no HTTPS
        }

        public bool Login(string username, string password)
        {
            // No hashing, plaintext password comparison
            if (username == "admin" && password == "admin123")
            {
                _loggedInUsers.Add(username);
                return true;
            }
            return false;
        }

        public List<Product> GetProducts()
        {
            // Blocking async call - sync over async anti-pattern
            var response = _http.GetAsync("/api/products").Result;
            var json = response.Content.ReadAsStringAsync().Result;

            // Returning internal mutable cache directly
            return _productCache.Values.ToList();
        }

        public void AddProduct(Product p)
        {
            // No validation, no thread-safety
            _productCache[p.Id] = p;
        }

        public Product GetProductById(int id)
        {
            // No null check
            return _productCache[id];
        }

        public void DeleteProduct(int id)
        {
            _productCache.Remove(id);
            Console.WriteLine("Product deleted: " + id);
        }

        public List<string> GetAllLoggedInUsers()
        {
            return _loggedInUsers; // exposing internal list
        }
    }
}
