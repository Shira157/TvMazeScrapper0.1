using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TvMazeScrapper.Services;
using Microsoft.Extensions.Logging;

namespace TvMazeScrapper.Tests.Services
{
    public class FetchAllShowsAsyncServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<FetchAllShowsAsyncService>> _mockLogger;
        private readonly FetchAllShowsAsyncService _service;

        public FetchAllShowsAsyncServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new System.Uri("https://api.tvmaze.com/")
            };
            _mockLogger = new Mock<ILogger<FetchAllShowsAsyncService>>();
            _service = new FetchAllShowsAsyncService(_httpClient, _mockLogger.Object);
        }

        [Fact]
        public async Task FetchAllShowsAsync_ReturnsShows_WhenApiReturnsValidData()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"id\":1,\"name\":\"Test Show\"}]")
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
            var result = await _service.FetchAllShowsAsync(cancellationToken);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Show", result[0].name);
            Assert.Equal(1, result[0].id);
        }

        [Fact]
        public async Task FetchAllShowsAsync_ReturnsEmptyList_WhenApiReturnsNoData()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
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
            var result = await _service.FetchAllShowsAsync(cancellationToken);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FetchAllShowsAsync_ReturnsEmptyList_WhenApiResponseIsError()
        {
            // Arrange
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
            var result = await _service.FetchAllShowsAsync(cancellationToken);

            // Assert
            Assert.Empty(result);
        }
    }

}
