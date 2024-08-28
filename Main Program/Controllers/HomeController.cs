using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using TvMazeScrapper.Data;
using TvMazeScrapper.Models;
using TvMazeScrapper.Models.ViewModels;
using TvMazeScrapper.Services;

namespace TvMazeScrapper.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FetchAllShowsAsyncService _fetchAllShowsService;
        private readonly FetchCastForShowAsyncService _fetchCastForShowService;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, FetchAllShowsAsyncService fetchAllShowsAsyncService, FetchCastForShowAsyncService fetchCastForShowAsyncService, ApplicationDbContext db)
        {
            _logger = logger;
            _fetchAllShowsService = fetchAllShowsAsyncService;
            _fetchCastForShowService = fetchCastForShowAsyncService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var showMetaData = await _db.ShowMetaData.FirstOrDefaultAsync();

            if (showMetaData == null || string.IsNullOrEmpty(showMetaData.JsonData))
            {
                ViewBag.Message = "No data available.";
                return View(new ShowsListViewModel { Shows = new List<ShowViewModel>() });
            }

            try
            {
                // Deserialize the JSON data into the ShowsContainer, which matches the JSON structure
                var container = JsonConvert.DeserializeObject<ShowsContainer>(showMetaData.JsonData);

                // If the container or the Shows list is null, handle it appropriately
                if (container?.Shows == null)
                {
                    ViewBag.Message = "No data available.";
                    return View(new ShowsListViewModel { Shows = new List<ShowViewModel>() });
                }

                // Create a ViewModel to pass to the view
                var viewModel = new ShowsListViewModel
                {
                    Shows = container.Shows
                };

                return View(viewModel);
            }
            catch (JsonSerializationException ex)
            {
                // Log the exception or handle it as needed
                ViewBag.Message = "Error processing data: " + ex.Message;
                return View(new ShowsListViewModel { Shows = new List<ShowViewModel>() });
            }
        }

        

        
    }
}
