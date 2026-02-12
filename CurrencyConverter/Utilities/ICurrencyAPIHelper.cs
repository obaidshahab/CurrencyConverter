using CurrencyConverter.Models;

namespace CurrencyConverter.Utilities
{
    public interface ICurrencyAPIHelper
    {
        Task<CurrencyAPIModel> GetExchangeRates(string baseCurrency);
        Task<Dictionary<string, string>> GetSupportedCurrencies();
        Task<Decimal> CalculateExchangeRate(string baseCurrency, string toCurrency);

        Task<HistoricalExchangeRateResponseModel> GetHistoricalExchangeRates(HistoricalExchangeRateRequestModel request);
    }
}
