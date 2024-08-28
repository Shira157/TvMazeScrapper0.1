using Polly;
using Polly.Retry;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TvMazeScrapper.Models;

namespace TvMazeScrapper.Services
{
    public class FetchAllShowsAsyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FetchAllShowsAsyncService> _logger;
        private readonly SemaphoreSlim _throttle = new SemaphoreSlim(19);
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public FetchAllShowsAsyncService(HttpClient httpClient, ILogger<FetchAllShowsAsyncService> logger )
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
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

        public async Task<List<Show>> FetchAllShowsAsync(CancellationToken cancellationToken)
        {
            var allShows = new ConcurrentBag<Show>(); // Use thread-safe collection
            bool moreShowsAvailable = true;
            int maxPagesToFetch = int.MaxValue; // Set a reasonable upper limit if needed

            // Use Parallel.ForEachAsync for optimized parallel processing
            await Parallel.ForEachAsync(Enumerable.Range(0, maxPagesToFetch), cancellationToken, async (page, token) =>
            {
                if (!moreShowsAvailable)
                {
                    return; // Exit early if no more shows are available
                }

                await _throttle.WaitAsync(token); // Control concurrency using SemaphoreSlim

                try
                {
                    // Fetch HTTP response with retries using Polly
                    var response = await _retryPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"https://api.tvmaze.com/shows?page={page}", token)
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        // Process the response directly
                        using var stream = await response.Content.ReadAsStreamAsync(token);
                        var showsPage = await JsonSerializer.DeserializeAsync<List<Show>>(stream, cancellationToken: token);

                        if (showsPage != null && showsPage.Count > 0)
                        {
                            foreach (var show in showsPage)
                            {
                                allShows.Add(show); // Directly add to thread-safe collection
                            }
                        }
                        else
                        {
                            // No more shows available; signal to stop further processing
                            moreShowsAvailable = false;
                            token.ThrowIfCancellationRequested();
                        }
                    }
                    else
                    {
                        _logger.LogError($"Failed to fetch shows for page {page}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while processing page {page}");
                }
                finally
                {
                    _throttle.Release(); // Release SemaphoreSlim
                }
            });

            return allShows.ToList();
        }

        private async Task<List<Show>> FetchPageAsync(int page, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.tvmaze.com/shows?page={page}");

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    // Handle rate limit exceeded
                    _logger.LogWarning("Rate limit exceeded. Waiting before retrying.");
                    await Task.Delay(10000); // Wait for 10 seconds before retrying
                    return await FetchPageAsync(page, cancellationToken); // Retry the same request
                }

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    return await JsonSerializer.DeserializeAsync<List<Show>>(stream, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch shows for page {page}");
            }
            return null; // Return null to indicate a failure
        }

        public async Task<List<Show>> FetchFirstShowPageAsync(CancellationToken cancellationToken)
        {
            var allShows = new List<Show>();

            bool moreShowsAvailable = true;

            while (moreShowsAvailable)
            {
                var response = await _httpClient.GetAsync($"https://api.tvmaze.com/shows?page=0");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);


                    var showsPage = JsonSerializer.Deserialize<List<Show>>(content);

                    if (showsPage != null && showsPage.Count > 0)
                    {
                        allShows.AddRange(showsPage);
                        //page++;
                        moreShowsAvailable = false; //temp
                    }
                    else
                    {
                        moreShowsAvailable = false;
                    }
                }
                else
                {

                    moreShowsAvailable = false;
                }
            }



            return allShows;
        }
    }

    

        
    
}

