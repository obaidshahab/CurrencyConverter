using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverter.Utilities
{
    public class CacheHelper:ICacheHelper
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheHelper> _logger;
        public CacheHelper(IMemoryCache cache, ILogger<CacheHelper> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T GetCacheByKey<T>(string key) {
        
            _cache.TryGetValue(key,out T result);

            return result;
        }

        public void SetCache<T>(string key, T value)
        {
            
             _cache.Set(key, value,TimeSpan.FromMinutes(5));
            
        }

    }
}
