namespace CurrencyConverter.Utilities
{
    public interface ICacheHelper
    {
        T GetCacheByKey<T>(string key);
        void SetCache<T>(string key, T value);
    }
}
