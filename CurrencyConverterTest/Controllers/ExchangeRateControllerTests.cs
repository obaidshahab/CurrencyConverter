using Microsoft.VisualStudio.TestTools.UnitTesting;
using CurrencyConverter.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter.Models;
using CurrencyConverter.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrencyConverter.Controllers.Tests
{
    [TestClass]
    public class ExchangeRateControllerTests
    {
        private Mock<ICurrencyAPIHelper> _helperMock;
        private Mock<ILogger<ExchangeRateController>> _loggerMock;
        private ExchangeRateController _controller;

        private Dictionary<string, string> SupportedCurrencies =>
            new()
            {
            { "EUR", "Euro" },
            { "USD", "US Dollar" },
            { "INR", "Indian Rupee" }
            };

        [TestInitialize]
        public void Setup()
        {
            _helperMock = new Mock<ICurrencyAPIHelper>();
            _loggerMock = new Mock<ILogger<ExchangeRateController>>();

            _controller = new ExchangeRateController(
                _helperMock.Object,
                _loggerMock.Object);
        }

        // -------------------------------
        // GetLatestExchangeRate
        // -------------------------------

        [TestMethod]
        public async Task GetLatestExchangeRate_ReturnsOk_WhenCurrencySupported()
        {
            var model = new CurrencyAPIModel();

            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(SupportedCurrencies);

            _helperMock.Setup(x => x.GetExchangeRates("EUR"))
                .ReturnsAsync(model);

            var result = await _controller.GetLatestExchangeRate("EUR");

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetLatestExchangeRate_ReturnsNotFound_WhenCurrencyUnsupported()
        {
            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(new Dictionary<string, string>());

            var result = await _controller.GetLatestExchangeRate("ABC");

            var notFound = result as NotFoundObjectResult;

            Assert.IsNotNull(notFound);
            Assert.AreEqual("Currency not supported", notFound.Value);
        }

        // -------------------------------
        // GetSupportedCurrencies
        // -------------------------------

        [TestMethod]
        public async Task GetSupportedCurrencies_ReturnsOk()
        {
            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(SupportedCurrencies);

            var result = await _controller.GetSupportedCurrencies();

            var ok = result as OkObjectResult;

            Assert.IsNotNull(ok);
            Assert.AreEqual(3, ((Dictionary<string, string>)ok.Value).Count);
        }

        // -------------------------------
        // ConvertCurrency
        // -------------------------------

        [TestMethod]
        public async Task ConvertCurrency_ReturnsOk_WhenRateFound()
        {
            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(SupportedCurrencies);

            _helperMock.Setup(x => x.CalculateExchangeRate("USD", "INR"))
                .ReturnsAsync(83m);

            var result = await _controller.ConvertCurrency("usd", "inr");

            var ok = result as OkObjectResult;

            Assert.IsNotNull(ok);
            Assert.AreEqual(83m, ok.Value);
        }

        [TestMethod]
        public async Task ConvertCurrency_ReturnsBadRequest_WhenUnsupported()
        {
            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(new Dictionary<string, string>());

            var result = await _controller.ConvertCurrency("USD", "XXX");

            var bad = result as BadRequestObjectResult;

            Assert.IsNotNull(bad);
            Assert.AreEqual("Currency not supported", bad.Value);
        }

        [TestMethod]
        public async Task ConvertCurrency_ReturnsBadRequest_WhenRateNotFound()
        {
            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(SupportedCurrencies);

            _helperMock.Setup(x => x.CalculateExchangeRate("USD", "INR"))
                .ReturnsAsync(0m);

            var result = await _controller.ConvertCurrency("USD", "INR");

            var bad = result as BadRequestObjectResult;

            Assert.IsNotNull(bad);
            Assert.AreEqual("Exchange rate not found", bad.Value);
        }

        // -------------------------------
        // Historical Exchange
        // -------------------------------

        [TestMethod]
        public async Task GetHistoricalExchangeRates_ReturnsOk()
        {
            var request = new HistoricalExchangeRateRequestModel
            {
                BaseCurrency = "usd",
                FromDate = DateTime.Parse("2025-01-01"),
                ToDate = DateTime.Parse("2025-01-02"),
                PageNumber = 1,
                Records = 10
            };

            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(SupportedCurrencies);

            _helperMock.Setup(x => x.GetHistoricalExchangeRates(It.IsAny<HistoricalExchangeRateRequestModel>()))
                .ReturnsAsync(new HistoricalExchangeRateResponseModel());

            var result = await _controller.GetHistoricalExchangeRates(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetHistoricalExchangeRates_ReturnsBadRequest_WhenDatesInvalid()
        {
            var request = new HistoricalExchangeRateRequestModel
            {
                BaseCurrency = "USD",
                FromDate = DateTime.Parse("2025-02-01"),
                ToDate = DateTime.Parse("2025-01-01")
            };

            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(SupportedCurrencies);

            var result = await _controller.GetHistoricalExchangeRates(request);

            var bad = result as BadRequestObjectResult;

            Assert.IsNotNull(bad);
            Assert.AreEqual("From Date cannot be greater than To Date", bad.Value);
        }

        [TestMethod]
        public async Task GetHistoricalExchangeRates_ReturnsBadRequest_WhenCurrencyUnsupported()
        {
            var request = new HistoricalExchangeRateRequestModel
            {
                BaseCurrency = "XXX",
                FromDate = DateTime.Parse("2025-01-01"),
                ToDate = DateTime.Parse("2025-01-02"),
                PageNumber = 1,
                Records=10

            };

            _helperMock.Setup(x => x.GetSupportedCurrencies())
                .ReturnsAsync(SupportedCurrencies);

            var result = await _controller.GetHistoricalExchangeRates(request);

            var notFound = result as BadRequestObjectResult;

            Assert.IsNotNull(notFound);
            Assert.AreEqual("Currency not supported", notFound.Value);
        }
    }
}