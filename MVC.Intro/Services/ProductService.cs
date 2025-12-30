using MVC.Intro.Data;
using MVC.Intro.Models;
using Microsoft.EntityFrameworkCore;

namespace MVC.Intro.Services
{
    public class ProductService
    {
        private const string ProductPrefix = "PRD_";
        private readonly ILogger<ProductService> _logger;
        private readonly AppDbContext _context;

        // Compiled query for faster repeated execution (read-only)
        private static readonly Func<AppDbContext, List<Product>> _getAllProductsCompiled =
            EF.CompileQuery((AppDbContext ctx) => ctx.Products.AsNoTracking().ToList());

        public ProductService(ILogger<ProductService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public List<Product> GetAllProducts()
        {
            return _getAllProductsCompiled(_context);
        }
        public Product GetProductById(Guid id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            return product;
        }

        public Product AddProduct(Product product)
        {
            var toAdd = new Product
            {
                Name = product.Name,
                Price = product.Price,
                ImagePath = product.ImagePath
            };
            _logger.LogInformation("Adding product: {ProductName} with price {ProductPrice}", toAdd.Name, toAdd.Price);
            if (product == null)
            {
                throw new ArgumentNullException("Product is null");
            }
            toAdd.Id = Guid.NewGuid();
            if (!toAdd.Name.StartsWith(ProductPrefix))
            {
                DecorateProductName(toAdd);
            }
            _context.Products.Add(toAdd);
            _context.SaveChanges();
            return toAdd;
        }
        public void DeleteProduct(Guid id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                throw new ArgumentNullException("Product not found");
            }
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var existing = _context.Products.FirstOrDefault(p => p.Id == product.Id);
            if (existing == null)
            {
                throw new ArgumentNullException("Product not found");
            }

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.ImagePath = product.ImagePath ?? existing.ImagePath;

            if (!existing.Name.StartsWith(ProductPrefix))
            {
                DecorateProductName(existing);
            }

            _context.SaveChanges();
        }

        private void DecorateProductName(Product product)
        {
            product.Name = ProductPrefix + product.Name;
        }

    }
}
