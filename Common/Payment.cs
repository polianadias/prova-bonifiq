namespace ProvaPub.Common
{
    public abstract class Payment
    {
        public abstract string Method { get; }
        public abstract bool Pay(decimal paymentValue, int customerId);
    }
}
