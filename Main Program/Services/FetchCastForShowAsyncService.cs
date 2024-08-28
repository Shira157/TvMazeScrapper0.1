using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TvMazeScrapper.Models;

namespace TvMazeScrapper.Services
{
    public class FetchCastForShowAsyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FetchCastForShowAsyncService> _logger;
        private readonly SemaphoreSlim _throttle = new SemaphoreSlim(19);
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public FetchCastForShowAsyncService(HttpClient httpClient, ILogger<FetchCastForShowAsyncService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _retryPolicy = Policy
                 .HandleResult<HttpResponseMessage>(r => r.StatusCode == (System.Net.HttpStatusCode)429)
                .Or<IOException>()
                .Or<SocketException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Rate limit exceeded. Retrying in {timeSpan.Seconds} seconds... Attempt {retryCount}");
                });
        }

        public async Task<List<CastPerson>> FetchCastForShowAsync(int showId, CancellationToken cancellationToken)
        {
            await _throttle.WaitAsync(cancellationToken); // Control concurrency to respect the rate limit

            try
            {
                // Fetch HTTP response with retries using Polly
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"https://api.tvmaze.com/shows/{showId}/cast", cancellationToken)
                );

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var castMembers = await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream, cancellationToken: cancellationToken);

                    var castPersonList = castMembers?.Select(castMember =>
                    {
                        var person = castMember.GetProperty("person");

                        // Get the person ID, name, and birthday
                        var id = person.GetProperty("id").GetInt32();
                        var name = person.GetProperty("name").GetString();
                        var birthdayString = person.GetProperty("birthday").GetString();
                        DateTime? birthday = null;

                        // Parse the birthday if it's not null or empty
                        if (!string.IsNullOrEmpty(birthdayString))
                        {
                            birthday = DateTime.Parse(birthdayString);
                        }

                        return new CastPerson
                        {
                            id = id,
                            name = name,
                            birthday = birthday
                        };
                    }).ToList();

                    return castPersonList ?? new List<CastPerson>();
                }
                else
                {
                    _logger.LogError($"Failed to fetch cast for show {showId}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching cast for show.");
            }
            finally
            {
                _throttle.Release(); // Ensure the semaphore is released even if an error occurs
            }

            return new List<CastPerson>();
        }
    }

    public class CastPersonWrapper
        {
            public CastPerson Person { get; set; }
        }
    
}
