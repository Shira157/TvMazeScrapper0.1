using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TvMazeScrapper.Services;

namespace TvMazeScrapper.Tests.Services
{
    public class FetchCastForShowAsyncServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<FetchCastForShowAsyncService>> _mockLogger;
        private readonly FetchCastForShowAsyncService _service;

        public FetchCastForShowAsyncServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new System.Uri("https://api.tvmaze.com/")
            };
            _mockLogger = new Mock<ILogger<FetchCastForShowAsyncService>>();
            _service = new FetchCastForShowAsyncService(_httpClient, _mockLogger.Object);
        }

        [Fact]
        public async Task FetchCastForShowAsync_ReturnsCastPersons_WhenApiReturnsValidData()
        {
            // Arrange
            var showId = 1;
            var jsonResponse = "[{\"person\":{\"id\":1,\"name\":\"John Doe\",\"birthday\":\"2000-01-01\"}}]";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var cancellationToken = CancellationToken.None;
            // Act
            var result = await _service.FetchCastForShowAsync(showId, cancellationToken);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].id);
            Assert.Equal("John Doe", result[0].name);
            Assert.Equal(new System.DateTime(2000, 1, 1), result[0].birthday);
        }

        [Fact]
        public async Task FetchCastForShowAsync_ReturnsEmptyList_WhenApiResponseIsError()
        {
            // Arrange
            var showId = 1;
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var cancellationToken = CancellationToken.None;
            // Act
            var result = await _service.FetchCastForShowAsync(showId, cancellationToken);

            // Assert
            Assert.Empty(result);
        }
    }

}
