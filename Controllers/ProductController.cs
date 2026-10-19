
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OnlineShoppingSystem.Models;
using System.Net.Http.Json;

namespace OnlineShoppingSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public ProductController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        private async Task<List<Product>> GetProducts()
        {
            return await _cache.GetOrCreateAsync("products", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(10);

                var client = _httpClientFactory.CreateClient();

                var data = await client.GetFromJsonAsync<List<ApiProduct>>(
                    "https://fakestoreapi.com/products");

                return data?.Select(p => new Product
                {
                    ProductId = p.Id,
                    ProductName = p.Title,
                    Category = p.Category,
                    Price = p.Price,
                    DiscountPercentage = 10
                }).ToList() ?? new List<Product>();
            }) ?? new List<Product>();
        }

        public async Task<IActionResult> Index()
        {
            var products = await GetProducts();

            ViewBag.CustomerName =
                Request.Cookies["CustomerName"] ?? "";

            ViewBag.SelectedCategory =
                HttpContext.Session.GetString("Category") ?? "All";

            return View(products);
        }

        [HttpPost]
        public IActionResult SavePreferences(
            string customerName, string category)
        {
            Response.Cookies.Append("CustomerName", customerName,
                new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(7)
                });

            HttpContext.Session.SetString("Category", category);

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 60,
            Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Details(int productId)
        {
            var products = await GetProducts();

            var product = products.FirstOrDefault(
                p => p.ProductId == productId);

            if (product == null)
                return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Order(int productId)
        {
            var products = await GetProducts();

            var product = products.FirstOrDefault(
                p => p.ProductId == productId);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int productId)
        {
            var products = await GetProducts();

            var product = products.FirstOrDefault(
                p => p.ProductId == productId);

            if (product == null)
                return NotFound();

            ViewBag.Message = "Order submitted successfully!";
            return View("Order", product);
        }

        public class ApiProduct
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Price { get; set; }
        }
    }
}
