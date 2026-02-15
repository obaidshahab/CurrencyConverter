using Asp.Versioning;
using CurrencyConverter.Models;
using CurrencyConverter.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Controllers
{
 
   
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    //[Route("api/v{version:apiVersion}/[controller]")]
    
    [Authorize(Roles ="Admin")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ILogger<ExchangeRateController> logger;
        private readonly ICurrencyAPIHelper currencyAPIHelper;

        // this list can be moved to config file or in db if we want to add more unsupported currencies in future

        private string[] nonSuppportedCurrencies = new string[] { "TRY", "PLN", "THB", "MXN" };
        public ExchangeRateController(ICurrencyAPIHelper _currencyAPIHelper,ILogger<ExchangeRateController> _logger) { 
            currencyAPIHelper = _currencyAPIHelper;
            this.logger = _logger;
        }

        /// <summary>
        /// This method is used to get current exchange rate with base currency as EUR as default
        /// </summary>
        /// <param name="currencyCode"></param
        /// <param name="baseCurrency"></param
        /// <returns></returns>
        [HttpGet("GetLatestExchangeRate")]
        public async Task<IActionResult> GetLatestExchangeRate( string baseCurrency="EUR")
        {
            var getSupportedCurrency = await currencyAPIHelper.GetSupportedCurrencies();
            if (getSupportedCurrency.Count==0 || !getSupportedCurrency.ContainsKey(baseCurrency) ){ 
                return NotFound("Currency not supported");
            }

            var result = await currencyAPIHelper.GetExchangeRates(baseCurrency);

            return Ok(result);
            //if (string.IsNullOrEmpty(currencyCode) || nonSuppportedCurrencies.Contains(currencyCode))
            //{
            //    logger.LogInformation("Currency Code not supported");
            //    return BadRequest("Currency Code not supported");
            //}
            //else
            //{
            //    var result = await currencyAPIHelper.GetExchangeRates(baseCurrency);
            //    result.Rates.TryGetValue(currencyCode, out var rates);
            //    if (rates > 0)
            //        return Ok(rates);
            //    else
            //    {
            //        return BadRequest("Exchange rate not available");
            //    }
            //}
        }

        /// <summary>
        /// This method is to get all the currencies that are supported by the exchange service
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSupportedCurrencies")]
        public async Task<IActionResult> GetSupportedCurrencies()
        {
            var result= await currencyAPIHelper.GetSupportedCurrencies();
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromCurrency"></param>
        /// <param name="toCurrency"></param>
        /// <returns></returns>
        [HttpGet("ConvertCurrency")]
        public async Task<IActionResult> ConvertCurrency(string fromCurrency, string toCurrency)
        {
            fromCurrency = fromCurrency.ToUpper();
            toCurrency = toCurrency.ToUpper();
            var getSupportedCurrency = await currencyAPIHelper.GetSupportedCurrencies();
            if (getSupportedCurrency.Count == 0 || !getSupportedCurrency.ContainsKey(fromCurrency)
                || !getSupportedCurrency.ContainsKey(toCurrency))
            {
                return BadRequest("Currency not supported");
            }

            var result = await currencyAPIHelper.CalculateExchangeRate(fromCurrency, toCurrency);
            if (result > 0)
                return Ok(result);
            else
                return BadRequest("Exchange rate not found");



        }

        [HttpPost("GetHistoricalExchangeRates")]
        public async Task<IActionResult> GetHistoricalExchangeRates(HistoricalExchangeRateRequestModel requestModel)
        {
            if (requestModel is null)
            {
                return BadRequest("Request body is required.");
            }
            if (requestModel.PageNumber<=0 || requestModel.Records <= 0)

            {
                return BadRequest("Page Number and Records must be greater than 0");
            }

            requestModel.BaseCurrency = requestModel.BaseCurrency.ToUpper();
            var getSupportedCurrency = await currencyAPIHelper.GetSupportedCurrencies();
            if (getSupportedCurrency.Count == 0 || !getSupportedCurrency.ContainsKey(requestModel.BaseCurrency))
            {
                return BadRequest("Currency not supported");
            }

            if (requestModel.FromDate.Date > requestModel.ToDate.Date)
            {
                return BadRequest("From Date cannot be greater than To Date");
            }

            var result = await currencyAPIHelper.GetHistoricalExchangeRates(requestModel);
            return Ok(result);

        }
    }
}
