using Microsoft.VisualStudio.TestTools.UnitTesting;
using CurrencyConverter.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System.Net;
using System.Text.Json;

namespace CurrencyConverter.Utilities.Tests
{
    [TestClass]
    public class CurrencyAPIHelperTests
    {
        private Mock<ILogger<CurrencyAPIHelper>> _loggerMock;
        private Mock<ICacheHelper> _cacheMock;
        private Mock<IConfiguration> _configMock;
        private Mock<IHttpClientFactory> _factoryMock;

        private CurrencyAPIHelper _helper;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<CurrencyAPIHelper>>();
            _cacheMock = new Mock<ICacheHelper>();
            _configMock = new Mock<IConfiguration>();
            _factoryMock = new Mock<IHttpClientFactory>();

            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(x => x["BaseUrl"]).Returns("https://fake-api/");
            _configMock.Setup(x => x.GetSection("CurrencyProvider"))
                       .Returns(sectionMock.Object);
        }

        private void SetupHttpClient(object responseObject)
        {
            var json = JsonSerializer.Serialize(responseObject);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            _factoryMock.Setup(x => x.CreateClient("ExternalApi")).Returns(client);

            _helper = new CurrencyAPIHelper(
                _loggerMock.Object,
                _cacheMock.Object,
                _configMock.Object,
                _factoryMock.Object);
        }

        [TestMethod]
        public async Task GetExchangeRates_ReturnsFromCache()
        {
            var model = new CurrencyAPIModel
            {
                Rates = new Dictionary<string, decimal> { { "INR", 83m } }
            };

            _cacheMock.Setup(x => x.GetCacheByKey<CurrencyAPIModel>("GetExchangeRates-USD"))
                      .Returns(model);

            SetupHttpClient(model);

            var result = await _helper.GetExchangeRates("USD");

            Assert.IsNotNull(result);
            Assert.AreEqual(83m, result.Rates["INR"]);
        }

        [TestMethod]
        public async Task GetExchangeRates_CallsApi_WhenCacheMiss()
        {
            var model = new CurrencyAPIModel
            {
                Rates = new Dictionary<string, decimal> { { "EUR", 0.92m } }
            };

            _cacheMock.Setup(x => x.GetCacheByKey<CurrencyAPIModel>(It.IsAny<string>()))
                      .Returns((CurrencyAPIModel)null);

            SetupHttpClient(model);

            var result = await _helper.GetExchangeRates("USD");

            Assert.AreEqual(0.92m, result.Rates["EUR"]);
            _cacheMock.Verify(x => x.SetCache(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task CalculateExchangeRate_ReturnsCorrectRate()
        {
            var model = new CurrencyAPIModel
            {
                Rates = new Dictionary<string, decimal> { { "INR", 80m } }
            };

            _cacheMock.Setup(x => x.GetCacheByKey<CurrencyAPIModel>(It.IsAny<string>()))
                      .Returns(model);

            SetupHttpClient(model);

            var rate = await _helper.CalculateExchangeRate("USD", "INR");

            Assert.AreEqual(80m, rate);
        }

        [TestMethod]
        public async Task GetSupportedCurrencies_ReturnsDictionary()
        {
            var data = new Dictionary<string, string>
        {
            { "USD", "US Dollar" }
        };

            _cacheMock.Setup(x => x.GetCacheByKey<Dictionary<string, string>>(It.IsAny<string>()))
                      .Returns((Dictionary<string, string>)null);

            SetupHttpClient(data);

            var result = await _helper.GetSupportedCurrencies();

            Assert.AreEqual("US Dollar", result["USD"]);
        }

        [TestMethod]
        public async Task GetHistoricalExchangeRates_ReturnsPaginatedResult()
        {
            var response = new HistoricalExchangeRateResponseModel
            {
                Amount = 1,
                Base = "USD",
                Rates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2025-01-01", new() { { "INR", 80 } } },
                { "2025-01-02", new() { { "INR", 81 } } },
                { "2025-01-03", new() { { "INR", 82 } } }
            }
            };

            _cacheMock.Setup(x => x.GetCacheByKey<HistoricalExchangeRateResponseModel>(It.IsAny<string>()))
                      .Returns((HistoricalExchangeRateResponseModel)null);

            SetupHttpClient(response);

            var request = new HistoricalExchangeRateRequestModel
            {
                BaseCurrency = "USD",
                FromDate = DateTime.Parse("2025-01-01"),
                ToDate = DateTime.Parse("2025-01-03"),
                PageNumber = 1,
                Records = 2
            };

            var result = await _helper.GetHistoricalExchangeRates(request);

            Assert.AreEqual(2, result.Rates.Count);
        }
    }
}