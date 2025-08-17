using ProvaPub.Common;
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

        public PagedList<Product> ListProducts(int page, int pageSize = 10) =>
            EntityHelper.ListEntities(_ctx.Products.AsQueryable(), page, pageSize);
    }
}
