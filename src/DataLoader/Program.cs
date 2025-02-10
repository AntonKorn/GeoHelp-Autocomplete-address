using DataLoader.Configuration;
using DataLoader.Extensions;
using DataLoader.Models.Osm;
using DataLoader.Services;
using GeoHelp.SeedContracts;
using GeoHelp.SeedContracts.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Linq;

IConfiguration? configuration = null;

var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

var serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton<IConfiguration>(configurationRoot);
serviceCollection.AddDataLoader();

serviceCollection.Configure<DataLoadConfiguration>(options => configuration?.GetSection(DataLoadConfiguration.SectionName).Bind(options));

var services = serviceCollection.BuildServiceProvider();

using (var scope = services.CreateScope())
{
    configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var api = scope.ServiceProvider.GetRequiredService<OverpassApiService>();
    var repository = scope.ServiceProvider.GetRequiredService<OverpassQueryRepository>();
    var transformation = scope.ServiceProvider.GetRequiredService<TransformationService>();

    var loaderConfiguration = scope.ServiceProvider.GetRequiredService<IOptions<DataLoadConfiguration>>().Value;

    OsmDocument<OsmCountry>? osmCountries = null;
    var osmCitiesByCountryCodeLookup = new Dictionary<string, OsmDocument<OsmCity>>();

    if (loaderConfiguration.ReloadAllCitiesAndCountries)
    {
        osmCountries = repository.QueryCoutries().Result;

        var countryEntities = transformation.ToAdministrativeGeoEntities(osmCountries);

        Console.WriteLine($"Loaded countries: {countryEntities.Count()}");

        Directory.CreateDirectory("output");

        File.WriteAllText("output/countries.json", countryEntities.ToJson());

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach (var country in countryEntities)
        {
            Console.WriteLine($"Startng country: {country.CountryCode} {country.Name}");

            var osmCities = repository.QueryCities(country.CountryCode).Result;

            osmCitiesByCountryCodeLookup.Add(country.CountryCode, osmCities);

            var cityEntities = transformation.ToAdministrativeGeoEntities(osmCities, country.CountryCode);

            File.WriteAllText($"output/cities-{country.CountryCode}.json", cityEntities.ToJson());

            Console.WriteLine($"Finished country: {country.CountryCode} {country.Name}, elapsed: {stopwatch.Elapsed}");
            stopwatch.Restart();
        }
    }

    if (loaderConfiguration.CountryCodesReloadAddresses is not null
        && loaderConfiguration.CountryCodesReloadAddresses.Any())
    {
        Console.WriteLine("Loading addresses");

        var sw = new Stopwatch();

        foreach (var countryCode in loaderConfiguration.CountryCodesReloadAddresses)
        {
            Console.WriteLine($"Loading {countryCode}");

            sw.Restart();

            if (!osmCitiesByCountryCodeLookup.TryGetValue(countryCode, out var osmCities))
            {
                osmCities = repository.QueryCities(countryCode).Result;
            }

            sw.Stop();

            Console.WriteLine($"Elapsed loading cities for {countryCode}: {sw.ElapsedMilliseconds}");

            if (osmCities.Elements is null)
            {
                throw new InvalidOperationException("Osm cities elements were unset");
            }

            foreach (var cityElement in osmCities.Elements)
            {
                if (cityElement.Tags is null)
                {
                    throw new InvalidOperationException("City tags were unset");
                }

                var cityName = cityElement.Tags.Name;

                if (string.IsNullOrEmpty(cityName))
                {
                    throw new InvalidOperationException("City name was unset");
                }

                if (string.IsNullOrEmpty(cityElement.Id))
                {
                    throw new InvalidOperationException("City id was unset");
                }

                Console.WriteLine($"Loading {cityName}, {cityElement.Id}");

                sw.Restart();

                var highwaysResult = repository.QueryHighways(cityName).Result;

                Console.WriteLine($"Highways of {cityName} elapsed: {sw.Elapsed}");

                var osmHighways = highwaysResult.Elements;

                if (osmHighways is null)
                {
                    throw new InvalidOperationException("Street elements were unset");
                }

                var highwaysByNameLookup = osmHighways
                    .Where(highway => !string.IsNullOrEmpty(highway.Tags?.Name))
                    .GroupBy(highway => highway.Tags!.Name!)
                    .ToDictionary(group => group.Key, group => group.First());

                var streetBySearchNameLookup = new Dictionary<string, AdministrativeGeoEntity>();

                var buildings = new List<BuildingEntity>();

                var multipointBuildingsResult = repository.QueryMultipointBuildings(cityName).Result;

                if (multipointBuildingsResult.Elements is null)
                {
                    throw new InvalidOperationException("Multipoint building elemnts were unset");
                }

                Console.WriteLine($"Querying multipoint buildings of {cityName} elapsed: {sw.Elapsed}");

                var singlePointBuildingsResult = repository.QuerySinglePointBuildings(cityName).Result;

                if (singlePointBuildingsResult.Elements is null)
                {
                    throw new InvalidOperationException("Singlepoint building elemnts were unset");
                }

                Console.WriteLine($"Querying Singlepoint buildings of {cityName} elapsed: {sw.Elapsed}");

                foreach (var building in multipointBuildingsResult.Elements)
                {
                    if (building.Center is null)
                    {
                        throw new InvalidOperationException("Multipoint buildings coordinates were unset");
                    }

                    if (building.Tags is null)
                    {
                        throw new InvalidOperationException("Building tags were unset");
                    }

                    if (string.IsNullOrEmpty(building.Street)
                        || string.IsNullOrEmpty(building.HouseNumber))
                    {
                        throw new InvalidOperationException("Building address is not defined");
                    }

                    EnsureHighwayEntity(building.Street, out var highwayEntity);

                    var entity = new BuildingEntity(building.Id!, highwayEntity.Id, building.HouseNumber, building.Center.Lat, building.Center.Lon);

                    buildings.Add(entity);
                }

                Console.WriteLine($"Processing multipoint buildings of {cityName} elapsed: {sw.Elapsed}");

                foreach (var building in singlePointBuildingsResult.Elements)
                {
                    if (!building.Lat.HasValue || !building.Lon.HasValue)
                    {
                        throw new InvalidOperationException("Signlepoint building coordinates were unset");
                    }

                    if (building.Tags is null)
                    {
                        throw new InvalidOperationException("Building tags were unset");
                    }

                    if (string.IsNullOrEmpty(building.HouseNumber)
                        || string.IsNullOrEmpty(building.Street))
                    {
                        throw new InvalidOperationException("Building address is not defined");
                    }

                    EnsureHighwayEntity(building.Street, out var highwayEntity);

                    var entity = new BuildingEntity(building.Id!, highwayEntity.Id, building.Street, building.Lat.Value, building.Lon.Value);

                    buildings.Add(entity);
                }

                Console.WriteLine($"Processing Singlepoint buildings of {cityName} elapsed: {sw.Elapsed}");

                var streetsDocument = new StreetsDocument(cityElement.Id, cityName, streetBySearchNameLookup.Values);

                Directory.CreateDirectory("output/addresses");
                Directory.CreateDirectory($"output/addresses/{countryCode}");

                File.WriteAllText($"output/addresses/{countryCode}/{cityElement.Id}-streets.json", streetsDocument.ToJson());
                File.WriteAllText($"output/addresses/{countryCode}/{cityElement.Id}-buildings.json", buildings.ToJson());

                Console.WriteLine($"Saving Json of {cityName} elapsed: {sw.Elapsed}");

                void EnsureHighwayEntity(string streetName, out AdministrativeGeoEntity highwayEntity)
                {
                    if (streetBySearchNameLookup.TryGetValue(streetName, out highwayEntity!))
                    {
                        return;
                    }

                    if (!streetBySearchNameLookup.TryGetValue(streetName, out highwayEntity!))
                    {
                        if (highwaysByNameLookup.TryGetValue(streetName, out var osmHighway))
                        {
                            highwayEntity = transformation.ToStreet(osmHighway, countryCode);
                        }
                        else
                        {
                            highwayEntity = new AdministrativeGeoEntity(Guid.NewGuid().ToString(), countryCode, streetName, null, null, null, null, null, null);
                        }
                    }

                    streetBySearchNameLookup[streetName] = highwayEntity;
                }
            }
        }

        sw.Stop();
    }
}