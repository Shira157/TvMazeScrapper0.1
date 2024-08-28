using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TvMazeScrapper.Data;
using TvMazeScrapper.Models.ViewModels;
using TvMazeScrapper.Services;

namespace TvMazeScrapper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : Controller
    {
        private readonly FetchAllShowsAsyncService _fetchAllShowsService;
        private readonly FetchCastForShowAsyncService _fetchCastForShowService;
        private readonly ShowMetaDataService _showMetaDataService;
        private readonly ApplicationDbContext _db;
        public ShowsController(FetchAllShowsAsyncService fetchAllShowsService, FetchCastForShowAsyncService fetchCastForShowService, ShowMetaDataService showMetaDataService, ApplicationDbContext db)
        {
            _fetchAllShowsService = fetchAllShowsService;
            _fetchCastForShowService = fetchCastForShowService;
            _showMetaDataService = showMetaDataService;
            _db = db;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            

            if (TempData.ContainsKey("ShowsJson"))
            {
                var showsJson = TempData["ShowsJson"] as string;
                return View(model: showsJson);
            }

            return View();
        }

        [HttpGet("get-all-shows")]
        public async Task<IActionResult> GetAllShows(CancellationToken cancellationToken) 
        {
            var allShows = await _fetchAllShowsService.FetchAllShowsAsync(cancellationToken);
            var fetchCastTasks = allShows.Select(show => _fetchCastForShowService.FetchCastForShowAsync(show.id, cancellationToken)).ToArray();

            // Execute all fetch tasks in parallel
            var allCasts = await Task.WhenAll(fetchCastTasks);

            

            var viewModel = new ShowsListViewModel
            {
                Shows = allShows.Select((show, index) => new ShowViewModel
                {
                    Id = show.id,
                    Name = show.name,
                    Cast = allCasts[index].OrderByDescending(c => c.birthday).Select(c => new CastMemberViewModel
                    {
                        Id = c.id,
                        Name = c.name,
                        Birthday = c.birthday
                    }).ToList()
                }).ToList()
            };

            var showJson = JsonConvert.SerializeObject(viewModel);

            await _showMetaDataService.SaveShowMetaDataAsync(showJson);

            return View("Index", viewModel);
        }

        [HttpGet("get-first-show-page")]
        public async Task<IActionResult> GetFirstShowPage(CancellationToken cancellationToken)
        {
            try
            {
                var allShows = await _fetchAllShowsService.FetchFirstShowPageAsync(cancellationToken);
                //var result = new List<object>();
                var viewModel = new ShowsListViewModel();

                foreach (var show in allShows)
                {
                    var cast = await _fetchCastForShowService.FetchCastForShowAsync(show.id, cancellationToken);


                    var showViewModel = new ShowViewModel
                    {
                        Id = show.id,
                        Name = show.name,
                        Cast = cast.OrderByDescending(c => c.birthday).Select(c => new CastMemberViewModel
                        {
                            Id = c.id,
                            Name = c.name,
                            Birthday = c.birthday
                        }).ToList()
                    };

                    

                    viewModel.Shows.Add(showViewModel);
                }

                //var showsJson = JsonConvert.SerializeObject(result, Formatting.Indented);

                var showJson = JsonConvert.SerializeObject(viewModel);

                await _showMetaDataService.SaveShowMetaDataAsync(showJson);



                //return RedirectToAction("Index");

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception or set a breakpoint here
                Console.WriteLine(ex);
                return StatusCode(500, "Internal server error");
            }
        }



    }
}
