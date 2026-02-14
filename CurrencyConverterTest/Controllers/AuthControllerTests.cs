using Microsoft.VisualStudio.TestTools.UnitTesting;
using CurrencyConverter.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter.Utilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using CurrencyConverter.Models;

namespace CurrencyConverter.Controllers.Tests
{
    
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IJwtService> _jwtMock;
        private AuthController _controller;

        [TestInitialize]
        public void Setup()
        {
            _jwtMock = new Mock<IJwtService>();
            _controller = new AuthController(_jwtMock.Object);
        }

        [TestMethod]
        public void Login_ReturnsOk_WhenCredentialsValid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "123"
            };

            _jwtMock.Setup(x =>
                x.GenerateToken("1001", "admin", "Admin"))
                .Returns("fake-jwt-token");

            // Act
            var result = _controller.Login(request);

            // Assert
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);

            var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
            var dict = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, object>>(json);

            Assert.AreEqual("fake-jwt-token", dict["access_token"].ToString());
            Assert.AreEqual("3600", dict["expires_in"].ToString());

            _jwtMock.Verify(x =>
                x.GenerateToken("1001", "admin", "Admin"),
                Times.Once);
        }

        [TestMethod]
        public void Login_ReturnsUnauthorized_WhenUsernameInvalid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "unknown",
                Password = "123"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual("Invalid username", unauthorized.Value);

            _jwtMock.Verify(x =>
                x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void Login_ReturnsUnauthorized_WhenPasswordInvalid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "wrong"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual("Invalid password", unauthorized.Value);

            _jwtMock.Verify(x =>
                x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}