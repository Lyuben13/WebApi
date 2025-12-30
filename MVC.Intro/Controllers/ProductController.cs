using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    [Route("[controller]/[action]")]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly IWebHostEnvironment _env;

        public ProductController(ProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
            => View(_productService.GetAllProducts());

        [HttpGet("{id}")]
        public IActionResult Details(Guid id)
            => View(_productService.GetProductById(id));

        [HttpGet("{id}")]
        public IActionResult Edit(Guid id)
        {
            var product = _productService.GetProductById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(product);

            HandleImageUpload(product, imageFile);

            _productService.UpdateProduct(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
            => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            HandleImageUpload(product, imageFile);

            _productService.AddProduct(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            _productService.DeleteProduct(id);
            return RedirectToAction(nameof(Index));
        }

        private void HandleImageUpload(Product product, IFormFile? imageFile)
        {
            if (imageFile is not { Length: > 0 }) return;

            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(string.Empty, "Позволени са само JPG, JPEG, PNG или WEBP файлове.");
                return;
            }

            if (imageFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "Файлът е твърде голям (максимум 2MB).");
                return;
            }

            var fileName = $"{Guid.NewGuid()}{ext}";
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var fullPath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            product.ImagePath = $"/uploads/products/{fileName}";
        }
    }
}
