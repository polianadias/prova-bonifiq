namespace ProvaPub.Common
{
    public class PixPayment : Payment
    {
        public override string Method => "pix";
        public override bool Pay(decimal paymentValue, int customerId)
        {
            // Implementar lógica de pagamento via PIX
            return true;
        }
    }
}
