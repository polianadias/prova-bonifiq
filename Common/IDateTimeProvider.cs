namespace ProvaPub.Common
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
