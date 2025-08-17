using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public class RandomService
    {
        private readonly TestDbContext _ctx;

        public RandomService(TestDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<int> GetRandom()
        {
            int number;
            bool exists;
            do
            {
                number = Random.Shared.Next(100);
                exists = await _ctx.Numbers.AnyAsync(x => x.Number == number);
            }
            while (exists);
            _ctx.Numbers.Add(new RandomNumber() { Number = number });
            await _ctx.SaveChangesAsync();
            return number;
        }
    }
}
