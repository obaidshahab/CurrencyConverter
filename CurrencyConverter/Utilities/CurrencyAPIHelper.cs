using CurrencyConverter.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Utilities
{
    public class CurrencyAPIHelper : ICurrencyAPIHelper
    {
        private readonly ILogger<CurrencyAPIHelper> _logger;
        private readonly ICacheHelper _cache;
        string baseUrl = "https://api.frankfurter.dev/v1/";
        public CurrencyAPIHelper(ILogger<CurrencyAPIHelper> logger, ICacheHelper cache)
        {
            _logger = logger;
            _cache = cache;
        }
        public async Task<CurrencyAPIModel> GetExchangeRates(string baseCurrency)
        {
            try
            {
                var result = new CurrencyAPIModel();
                var cacheKey = $"GetExchangeRates-{baseCurrency}";
                result = _cache.GetCacheByKey<CurrencyAPIModel>(cacheKey);
                if(result is not null)
                    return result;
                using (var httpClient = new HttpClient())
                {
                    var uri = $"{baseUrl}latest?base={baseCurrency}";

                    var response = await httpClient.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {

                        var content = await response.Content.ReadAsStringAsync();
                        result = JsonSerializer.Deserialize<CurrencyAPIModel>(content);
                        _cache.SetCache(cacheKey, result);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        public async Task<Dictionary<string, string>> GetSupportedCurrencies()
        {

            try
            {
                var result = new Dictionary<string, string>();
                string cacheKey = "supportedCurrencies";
                result = _cache.GetCacheByKey<Dictionary<string, string>> ("cacheKey");

                using (var httpClient = new HttpClient())
                {
                    var uri = $"{baseUrl}currencies";

                    var response = await httpClient.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {

                        var content = await response.Content.ReadAsStringAsync();
                        result = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                        _cache.SetCache(cacheKey, result);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        public async Task<Decimal> CalculateExchangeRate(string baseCurrency, string toCurrency)
        {
            var result = await GetExchangeRates(baseCurrency);
            decimal rate = 0m;
            if (result is not null)
            {
                result.Rates.TryGetValue(toCurrency, out rate);

            }
            return rate;
        }

        public async Task<HistoricalExchangeRateResponseModel> GetHistoricalExchangeRates(HistoricalExchangeRateRequestModel request)
        {
            HistoricalExchangeRateResponseModel result = new();
            if (request is not null)
            {

                try
                {
                    string _cacheKey= $"HistoricalRate-{request.BaseCurrency}-{request.FromDate.Date}-{request.ToDate.Date}";
                    result= _cache.GetCacheByKey<HistoricalExchangeRateResponseModel>(_cacheKey);
                    if (result != null)
                    {
                        var filteredResult = new HistoricalExchangeRateResponseModel
                        {
                            Amount = result.Amount,
                            Base = result.Base,
                            Start_date = result.Start_date,
                            End_date = result.End_date,
                            Rates = result.Rates.Skip((request.PageNumber - 1) * request.Records).Take(request.Records).ToDictionary(),

                        };
                        return filteredResult;
                    }

                    var uri = $"{baseUrl}{request.FromDate.ToString("yyyy-MM-dd")}..{request.ToDate.ToString("yyyy-MM-dd")}?base={request.BaseCurrency.ToUpper()}";
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync(uri);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            result = JsonSerializer.Deserialize<HistoricalExchangeRateResponseModel>(content);

                            if (result != null)
                            {
                                var filteredResult = new HistoricalExchangeRateResponseModel
                                {
                                    Amount = result.Amount,
                                    Base = result.Base,
                                    Start_date = result.Start_date,
                                    End_date = result.End_date,
                                    Rates = result.Rates.Skip((request.PageNumber - 1) * request.Records).Take(request.Records).ToDictionary(),

                                };
                                return filteredResult;
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    throw ex;
                }
            }
            return result;
        }
    }
}
