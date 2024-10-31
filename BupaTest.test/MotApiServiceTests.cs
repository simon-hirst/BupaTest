using Moq;
using Moq.Protected;
using Microsoft.Extensions.Configuration;
using BupaTest.Exceptions;
using BupaTest.Services;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace
 RegCheck.Tests
{
    public class MotApiServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockClientFactory;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly MotApiService _motApiService;

        public MotApiServiceTests()
        {
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockConfiguration = new Mock<IConfiguration>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            _motApiService = new MotApiService(
                mockHttpClient.Object,
                mockHttpContextAccessor.Object,
                _mockConfiguration.Object,
                _mockClientFactory.Object
            );
        }

        [Fact]
        public async Task GetMotData_ValidRegistration_ReturnsMotData()
        {
            // Arrange
            var registrationNumber = "AB12CDE";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object); // Create a concrete HttpClient

            _mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient); // Configure the mock

            var jsonResponse = @"
            [
                {
                    ""registration"": ""AB12CDE"",
                    ""make"": ""VAUXHALL"",
                    ""model"": ""CORSA"",
                    ""firstUsedDate"": ""2013.10.30"",
                    ""fuelType"": ""Petrol"",
                    ""primaryColour"": ""White"",
                    ""motTests"": [
                        {
                            ""completedDate"": ""2024.07.23 16:23:57"",
                            ""testResult"": ""PASSED"",
                            ""expiryDate"": ""2025.08.09"",
                            ""odometerValue"": ""98463"",
                            ""odometerUnit"": ""mi"",
                            ""motTestNumber"": ""206441576081"",
                            ""rfrAndComments"": []
                        }
                    ]
                }
            ]";

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())

                .ReturnsAsync(httpResponse);

            _mockConfiguration.Setup(x => x["MotApiKey"]).Returns("fZi8YcjrZN1cGkQeZP7Uaa4rTxua8HovaswPuIno");

            // Act
            var result = await _motApiService.GetMotData(registrationNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VAUXHALL", result.Make);
            Assert.Equal("CORSA", result.Model);
            // TODO: add more assertions for other properties
        }

        [Fact]
        public async Task GetMotData_InvalidRegistration_ThrowsMotApiException()
        {
            var registrationNumber = "invalid"; // Invalid registration number

            await Assert.ThrowsAsync<MotApiException>(async () =>
            {
                await _motApiService.GetMotData(registrationNumber);
            });
        }

        [Fact]
        public async Task GetMotData_ApiErrorResponse_ThrowsMotApiException()
        {
            var registrationNumber = "AB12CDE";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var errorResponse = @"{ ""code"": ""V10"", ""message"": ""Vehicle not found"" }"; // Example error response

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(errorResponse)
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _mockConfiguration.Setup(x => x["MotApiKey"]).Returns("your_api_key");

            var exception = await Assert.ThrowsAsync<MotApiException>(async () =>
            {
                await _motApiService.GetMotData(registrationNumber);
            });

            Assert.Equal("404", exception.ErrorCode);
            Assert.Equal("Vehicle not found", exception.Message);
        }

        [Fact]
        public async Task GetMotData_NetworkError_ThrowsMotApiException()
        {
            var registrationNumber = "AB12CDE";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new
         HttpRequestException("Network error"));

            _mockConfiguration.Setup(x => x["MotApiKey"]).Returns("your_api_key");

            await Assert.ThrowsAsync<MotApiException>(async () =>
            {
                await _motApiService.GetMotData(registrationNumber);
            });
        }

        [Fact]
        public async Task GetMotData_NoMotTestsFound_ThrowsMotApiException()
        {
            var registrationNumber = "AB12CDE";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var jsonResponse = @"
        [
            {
                ""registration"": ""AB12CDE"",
                ""make"": ""VAUXHALL"",
                ""model"": ""CORSA"",
                ""firstUsedDate"": ""2013.10.30"",
                ""fuelType"": ""Petrol"",
                ""primaryColour"": ""White"",
                ""motTests"": [] // Empty motTests array
            }
        ]";

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())

                .ReturnsAsync(httpResponse);

            _mockConfiguration.Setup(x => x["MotApiKey"]).Returns("your_api_key");

            await Assert.ThrowsAsync<MotApiException>(async () =>
            {
                await _motApiService.GetMotData(registrationNumber);
            });
        }

        [Fact]
        public async Task GetMotData_NullResponse_ThrowsMotApiException()
        {
            var registrationNumber = "AB12CDE";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = null // Null response content
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())

                .ReturnsAsync(httpResponse);

            _mockConfiguration.Setup(x => x["MotApiKey"]).Returns("your_api_key");

            await Assert.ThrowsAsync<MotApiException>(async () =>
            {
                await _motApiService.GetMotData(registrationNumber);
            });
        }

        [Theory]
        [InlineData("AB12 CDE")] // With spaces
        [InlineData("AB12-CDE")] // With hyphens
        [InlineData("ab12cde")] // Lowercase
        public async Task GetMotData_InvalidRegistrationFormat_ThrowsMotApiException(string registrationNumber)
        {
            await Assert.ThrowsAsync<MotApiException>(async () =>
            {
                await _motApiService.GetMotData(registrationNumber);
            });
        }
    }
}