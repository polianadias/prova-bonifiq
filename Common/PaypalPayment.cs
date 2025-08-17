namespace ProvaPub.Common
{
    public class PaypalPayment : Payment
    {
        public override string Method => "paypal";
        public override bool Pay(decimal paymentValue, int customerId)
        {
            // Implementar lógica de pagamento via paypal
            return true;
        }
    }
}
