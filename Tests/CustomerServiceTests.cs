using Microsoft.EntityFrameworkCore;
using ProvaPub.Common;
using ProvaPub.Models;
using ProvaPub.Repository;
using ProvaPub.Services;
namespace ProvaPub.Tests
{
    [TestClass]
    public class CustomerServiceTests
    {
        private sealed class FakeClock : IDateTimeProvider
        {
            public DateTime UtcNow { get; set; }
        }

        private static class DbFactory
        {
            public static TestDbContext NewCtx()
            {
                var opts = new DbContextOptionsBuilder<TestDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;
                return new TestDbContext(opts);
            }
        }
        private static CustomerService NewSut(TestDbContext ctx, DateTime utcNow) =>
            new CustomerService(ctx, new FakeClock { UtcNow = utcNow });

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task CanPurchase_Throws_When_CustomerId_Invalid(int invalidId)
        {
            using var ctx = DbFactory.NewCtx();
            var sut = NewSut(ctx, new DateTime(2025, 8, 18, 12, 0, 0, DateTimeKind.Utc));

            var ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => sut.CanPurchase(invalidId, 10));
            Assert.AreEqual("customerId", ex.ParamName);
        }
        [TestMethod]
        [DataRow(0)]
        [DataRow(-10)]
        public async Task CanPurchase_Throws_When_PurchaseValue_Invalid(decimal invalidValue)
        {
            using var ctx = DbFactory.NewCtx();
            var sut = NewSut(ctx, new DateTime(2025, 8, 18, 12, 0, 0, DateTimeKind.Utc));

            var ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => sut.CanPurchase(1, invalidValue));
            Assert.AreEqual("purchaseValue", ex.ParamName);
        }

        [TestMethod]
        public async Task CanPurchase_Throws_When_Customer_Not_Found()
        {
            using var ctx = DbFactory.NewCtx();
            var sut = NewSut(ctx, new DateTime(2025, 8, 18, 12, 0, 0, DateTimeKind.Utc));

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => sut.CanPurchase(999, 50));
            StringAssert.Contains(ex.Message, "does not exists");
        }

        [TestMethod]
        public async Task CanPurchase_False_When_Already_Bought_In_Last_Month()
        {
            using var ctx = DbFactory.NewCtx();

            ctx.Customers.Add(new Customer { Id = 1 });
            ctx.Orders.Add(new Order
            {
                CustomerId = 1,
                OrderDate = new DateTime(2025, 8, 10, 12, 0, 0, DateTimeKind.Utc),
                Value = 10
            });
            await ctx.SaveChangesAsync();

            var sut = NewSut(ctx, new DateTime(2025, 8, 18, 12, 0, 0, DateTimeKind.Utc));

            var can = await sut.CanPurchase(1, 50);
            Assert.IsFalse(can);
        }

        [TestMethod]
        public async Task CanPurchase_False_When_FirstPurchase_Over_100()
        {
            using var ctx = DbFactory.NewCtx();
            ctx.Customers.Add(new Customer { Id = 1 });
            await ctx.SaveChangesAsync();

            var sut = NewSut(ctx, new DateTime(2025, 8, 18, 12, 0, 0, DateTimeKind.Utc));
            var can = await sut.CanPurchase(1, 150m);
            Assert.IsFalse(can);
        }

        [TestMethod]
        [DataRow(DayOfWeek.Saturday)]
        [DataRow(DayOfWeek.Sunday)]
        public async Task CanPurchase_False_On_Weekends(DayOfWeek weekend)
        {
            using var ctx = DbFactory.NewCtx();
            ctx.Customers.Add(new Customer { Id = 1 });
            await ctx.SaveChangesAsync();

            var date = new DateTime(2025, 8, 16, 12, 0, 0, DateTimeKind.Utc); // sábado
            while (date.DayOfWeek != weekend) date = date.AddDays(1);

            var sut = NewSut(ctx, date);
            var can = await sut.CanPurchase(1, 50m);
            Assert.IsFalse(can);
        }

        [DataTestMethod]
        [DataRow(7)]   // antes das 08:00
        [DataRow(19)]  // depois das 18:00
        public async Task CanPurchase_False_Outside_Business_Hours(int hour)
        {
            using var ctx = DbFactory.NewCtx();
            ctx.Customers.Add(new Customer { Id = 1 });
            await ctx.SaveChangesAsync();

            var monday = new DateTime(2025, 8, 18, hour, 0, 0, DateTimeKind.Utc); // segunda
            var sut = NewSut(ctx, monday);

            var can = await sut.CanPurchase(1, 50m);
            Assert.IsFalse(can);
        }

        [TestMethod]
        public async Task CanPurchase_True_When_All_Rules_Pass()
        {
            using var ctx = DbFactory.NewCtx();
            ctx.Customers.Add(new Customer { Id = 1 });
            await ctx.SaveChangesAsync();

            var sut = NewSut(ctx, new DateTime(2025, 8, 18, 10, 0, 0, DateTimeKind.Utc));
            var can = await sut.CanPurchase(1, 100m);
            Assert.IsTrue(can);
        }

        [TestMethod]
        public void ListCustomers_Paginates()
        {
            using var ctx = DbFactory.NewCtx();

            ctx.Customers.AddRange(Enumerable.Range(1, 25).Select(i => new Customer { Id = i }));
            ctx.SaveChanges();

            var sut = NewSut(ctx, new DateTime(2025, 8, 18, 10, 0, 0, DateTimeKind.Utc));

            var page1 = sut.ListCustomers(1, 10);
            Assert.AreEqual(25, page1.TotalCount);
            Assert.IsTrue(page1.HasNext);
            Assert.AreEqual(10, page1.Items.Count);

            var page3 = sut.ListCustomers(3, 10);
            Assert.IsFalse(page3.HasNext);
            Assert.AreEqual(5, page3.Items.Count);
        }

        // (Opcional) Teste de borda 08:00 e 18:00
        [TestMethod]
        [DataRow(8)]
        [DataRow(18)]
        public async Task CanPurchase_True_On_Border_Hours(int hour)
        {
            using var ctx = DbFactory.NewCtx();
            ctx.Customers.Add(new Customer { Id = 1 });
            await ctx.SaveChangesAsync();

            var monday = new DateTime(2025, 8, 18, hour, 0, 0, DateTimeKind.Utc);
            var sut = NewSut(ctx, monday);

            var can = await sut.CanPurchase(1, 100m);
            Assert.IsTrue(can);
        }
    }
}
