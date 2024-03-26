using GeoHelp.Core;
using GeoHelp.Core.Entities;
using GeoHelp.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GeoHelp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GeoController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IGeoEntitySearchService _geoEntitySearchService;
        private readonly IConfiguration _configuration;

        public GeoController(
            DataContext dataContext,
            IGeoEntitySearchService geoEntitySearchService,
            IConfiguration configuration)
        {
            _dataContext = dataContext;
            _geoEntitySearchService = geoEntitySearchService;
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Country>> Countries(
            [FromQuery] string? searchTerm,
            [FromQuery] string? searchLocalesCommaSeparated,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0)
        {
            ValidateWindowSize(skip, take);

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
            [FromQuery] string? countryCode,
            [FromQuery] string? searchTerm,
            [FromQuery] string? searchLocalesCommaSeparated,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0)
        {
            ValidateWindowSize(skip, take);

            var citiesQuery = (IQueryable<City>)_dataContext.Get<City>().AsQueryable();

            if (!string.IsNullOrEmpty(countryCode))
            {
                citiesQuery = citiesQuery.Where(c => c.CountryCode == countryCode);
            }

            var result = _geoEntitySearchService.Query(
                citiesQuery,
                skip,
                take,
                GetSearchLocales(searchLocalesCommaSeparated),
                searchTerm ?? string.Empty);

            return Ok(result);
        }

        private void ValidateWindowSize(int _, int take)
        {
            if (take > int.Parse(_configuration["MaxTake"]))
            {
                throw new ArgumentException("Not supported window size");
            }
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
