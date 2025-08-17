using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public class ProductService
    {
        private readonly TestDbContext _ctx;

        public ProductService(TestDbContext ctx)
        {
            _ctx = ctx;
        }

        public ProductList ListProducts(int page, int pageSize = 10)
        {
            var totalCount = _ctx.Products.Count();
            var products = _ctx.Products.OrderBy(p => p.Id)
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            return new ProductList() { HasNext = page * pageSize < totalCount, TotalCount = totalCount, Products = products };
        }

    }
}
