using GeoHelp.Core;
using GeoHelp.Core.Entities;
using GeoHelp.Core.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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

        [HttpGet]
        public ActionResult<IEnumerable<Street>> Streets(
            [FromQuery] string? cityId,
            [FromQuery] string? countryCode,
            [FromQuery] string? searchTerm,
            [FromQuery] string? searchLocalesCommaSeparated,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0)
        {
            ValidateWindowSize(skip, take);

            var streetsQuery = (IQueryable<Street>)_dataContext.Get<Street>().AsQueryable();

            if (!string.IsNullOrEmpty(countryCode))
            {
                streetsQuery = streetsQuery.Where(c => c.CountryCode == countryCode);
            }

            if (!string.IsNullOrEmpty(cityId))
            {
                streetsQuery = streetsQuery.Where(street => street.CityId == cityId);
            }

            var result = _geoEntitySearchService.Query(
                streetsQuery,
                skip,
                take,
                GetSearchLocales(searchLocalesCommaSeparated),
                searchTerm ?? string.Empty);

            return Ok(result);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Building>> Buildings(
            [FromQuery] string streetId,
            [FromQuery] string? searchTerm,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0)
        {
            ValidateWindowSize(skip, take);

            ArgumentNullException.ThrowIfNull(streetId, nameof(streetId));

            var pipeline = new List<BsonDocument>()
            {
                new BsonDocument("$match", new BsonDocument(nameof(Street.OsmId), streetId))
            };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                pipeline.Add(new BsonDocument("$project", new BsonDocument
                {
                    {  nameof(Street.Buildings), new BsonDocument
                        {
                            { "$filter", new BsonDocument
                                {
                                    { "input", $"${nameof(Street.Buildings)}" },
                                    { "as", "building" },
                                    { "cond", new BsonDocument
                                        {
                                            { "$regexMatch", new BsonDocument
                                                {
                                                    { "input", $"$$building.{nameof(Building.HouseNumber)}" },
                                                    { "regex", searchTerm },
                                                    { "options", "i" } // Case-insensitive match
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }));
            }

            pipeline.Add(new BsonDocument("$project", new BsonDocument
                {
                    { nameof(Street.Buildings), new BsonDocument
                        {
                            { "$slice", new BsonArray { $"${nameof(Street.Buildings)}", skip, take } }
                        }
                    }
                }));

            var foundStreet = _dataContext.Get<Street>().Aggregate<Street>(pipeline).FirstOrDefault();

            if (foundStreet is null)
            {
                throw new InvalidOperationException("The street was not found");
            }

            return Ok(foundStreet.Buildings);
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
