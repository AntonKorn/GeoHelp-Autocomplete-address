using GeoHelp.Core;
using GeoHelp.Core.Entities;
using GeoHelp.Core.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace GeoHelp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GeoController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IGeoEntitySearchService _geoEntitySearchService;

        public GeoController(
            DataContext dataContext,
            IGeoEntitySearchService geoEntitySearchService)
        {
            _dataContext = dataContext;
            _geoEntitySearchService = geoEntitySearchService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Country>> Countries(
            [FromQuery] string? searchTerm,
            [FromQuery] string? searchLocalesCommaSeparated,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0)
        {
            var result = _geoEntitySearchService.Query(
                _dataContext.Get<Country>().AsQueryable(),
                skip,
                take,
                GetSearchLocales(searchLocalesCommaSeparated),
                searchTerm ?? string.Empty);

            return Ok(result);
        }

        [HttpGet]
        public ActionResult<IEnumerable<City>> Cities(
            [FromQuery] string countryCode,
            [FromQuery] string? searchTerm,
            [FromQuery] string? searchLocalesCommaSeparated,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0)
        {
            var result = _geoEntitySearchService.Query(
                _dataContext.Get<City>().AsQueryable().Where(c => c.CountryCode == countryCode),
                skip,
                take,
                GetSearchLocales(searchLocalesCommaSeparated),
                searchTerm ?? string.Empty);

            return Ok(result);
        }

        private string[] GetSearchLocales(string? searchLocalesCommaSeparated)
        {
            if (string.IsNullOrEmpty(searchLocalesCommaSeparated))
            {
                return new string[] { };
            }

            return searchLocalesCommaSeparated
                .Split(',')
                .Select(x => x.Trim())
                .ToArray();
        }
    }
}
