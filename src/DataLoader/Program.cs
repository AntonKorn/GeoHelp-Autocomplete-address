using DataLoader.Services;
using GeoHelp.SeedContracts.Extensions;
using System.Diagnostics;

var api = new OverpassApiService();
var repository = new OverpassQueryRepository(api);
var transformation = new TransformationService();

var osmCountries = repository.QueryCoutries().Result;

var countryEntities = transformation.ToGeoEntities(osmCountries);

Console.WriteLine($"Loaded countries: {countryEntities.Count()}");

Directory.CreateDirectory("output");

File.WriteAllText("output/countries.json", countryEntities.ToJson());

var stopwatch = new Stopwatch();
stopwatch.Start();
foreach (var country in countryEntities)
{
    Console.WriteLine($"Startng country: {country.CountryCode} {country.Name}");

    var cities = repository.QueryCities(country.CountryCode).Result;

    var cityEntities = transformation.ToGeoEntities(cities, country.CountryCode);

    File.WriteAllText($"output/cities-{country.CountryCode}.json", cityEntities.ToJson());

    Console.WriteLine($"Finished country: {country.CountryCode} {country.Name}, elapsed: {stopwatch.Elapsed}");
    stopwatch.Restart();
}