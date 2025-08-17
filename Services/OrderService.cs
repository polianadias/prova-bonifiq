using ProvaPub.Common;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public class OrderService
    {
        private readonly TestDbContext _ctx;
        private readonly Dictionary<string, Payment> _payments;

        public OrderService(TestDbContext ctx, IEnumerable<Payment> payments)
        {
            _ctx = ctx;
            _payments = payments.ToDictionary(p => p.Method, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<Order> PayOrder(string paymentMethod, decimal paymentValue, int customerId)
        {
            if (!_payments.TryGetValue(paymentMethod, out var payment))
                throw new NotSupportedException($"Método de pagamento '{paymentMethod}' não suportado.");

            payment.Pay(paymentValue, customerId);

            return await InsertOrder(new Order() //Retorna o pedido para o controller
            {
                Value = paymentValue
            });
        }

        public async Task<Order> InsertOrder(Order order)
        {
            order.OrderDate = DateTime.UtcNow.AddHours(-3);
            //Insere pedido no banco de dados
            return (await _ctx.Orders.AddAsync(order)).Entity;
        }
    }
}
