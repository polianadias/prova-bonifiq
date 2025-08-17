using ProvaPub.Models;

namespace ProvaPub.Common
{
    public static class EntityHelper
    {
        public static PagedList<T> ListEntities<T>(IQueryable<T> query, int page, int pageSize = 10) where T : class
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedList<T>
            {
                TotalCount = totalCount,
                HasNext = page * pageSize < totalCount,
                Items = items
            };
        }
    }
}
