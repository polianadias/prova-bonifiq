namespace ProvaPub.Common
{
    public class CreditCardPayment : Payment
    {
        public override string Method => "creditcard";
        public override bool Pay(decimal paymentValue, int customerId)
        {
            // Implementar lógica de pagamento via cartão de crédito
            return true;
        }
    }
}
