using GeoHelp.Core.Entities;
using GeoHelp.Core;
using GeoHelp.SeedContracts.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using GeoHelp.Core.Extensions;
using System.Diagnostics;
using DataSeeder.Configuration;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

IConfiguration? configuration = null;

var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

var serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton<IConfiguration>(configurationRoot);
serviceCollection.AddCore();

serviceCollection.Configure<SeederConfiguration>(options => configuration?.GetSection(SeederConfiguration.Section).Bind(options));

var services = serviceCollection.BuildServiceProvider();

using (var scope = services.CreateScope())
{
    configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var seederOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeederConfiguration>>().Value;

    var context = scope.ServiceProvider.GetService<DataContext>() ?? throw new InvalidOperationException(nameof(DataContext));

    var stopwatch = new Stopwatch();

    if (seederOptions.SeedCititesAndCountries)
    {
        stopwatch.Start();

        var countries = File.ReadAllText("Seeds/countries.json").FromAdministrativeJsonSet();

        context.Get<Country>().Indexes.DropAll();
        context.Get<City>().Indexes.DropAll();

        if (context.Get<Country>().AsQueryable().Count() != countries.Count())
        {
            context.Get<Country>().DeleteMany(Builders<Country>.Filter.Empty);

            foreach (var countriesChunk in countries.Chunk(500))
            {
                context.Get<Country>().InsertMany(
                    countriesChunk.Select(x => new Country()
                    {
                        Id = Guid.NewGuid(),
                        OsmId = x.Id,
                        CountryCode = x.CountryCode,
                        Name = x.Name,
                        NameEN = x.NameEN,
                        NameDE = x.NameDE,
                        NameRU = x.NameRU,
                        NameES = x.NameES,
                        NameFR = x.NameFR,
                        NameIT = x.NameIT,
                    }));
            }
        }

        Console.WriteLine($"Seed countries finished, elapsed {stopwatch.Elapsed}");

        foreach (var cityFile in Directory.EnumerateFiles("Seeds", "cities-*.json"))
        {
            stopwatch.Restart();

            var filename = Path.GetFileName(cityFile);

            Console.WriteLine($"Seedind {cityFile}");

            var countryCode = filename.Split('-')[1].Substring(0, 2);
            var cities = File.ReadAllText(cityFile).FromAdministrativeJsonSet();

            if (context.Get<City>().AsQueryable().Where(x => x.CountryCode == countryCode).Count() != cities.Count())
            {
                context.Get<City>().DeleteMany(Builders<City>.Filter.Eq(x => x.CountryCode, countryCode));

                foreach (var citiesChunk in cities.Chunk(500))
                {
                    context.Get<City>().InsertMany(
                        citiesChunk.Select(x => new City()
                        {
                            Id = Guid.NewGuid(),
                            OsmId = x.Id,
                            CountryCode = x.CountryCode,
                            Name = x.Name,
                            NameEN = x.NameEN,
                            NameDE = x.NameDE,
                            NameRU = x.NameRU,
                            NameES = x.NameES,
                            NameFR = x.NameFR,
                            NameIT = x.NameIT,
                        }));
                }
            }

            Console.WriteLine($"{cityFile} elapsed {stopwatch.Elapsed}");
        }

        stopwatch.Stop();

        AddIndexes<City>(context);
        AddIndexes<Country>(context);
    }

    if (seederOptions.CountryCodesSeedAddresses is not null
        && seederOptions.CountryCodesSeedAddresses.Any())
    {
        context.Get<Street>().Indexes.DropAll();
        context.Get<Building>().Indexes.DropAll();

        foreach (var countryCode in seederOptions.CountryCodesSeedAddresses)
        {
            foreach (var streetFile in Directory.EnumerateFiles($"Seeds/addresses/{countryCode}", "*-streets.json"))
            {
                stopwatch.Restart();

                var streetsDocument = File.ReadAllText(streetFile).FromStreetsDocumentJson();

                context.Get<Street>().DeleteMany(
                    Builders<Street>.Filter.And(
                        Builders<Street>.Filter.Eq(street => street.CountryCode, countryCode),
                        Builders<Street>.Filter.Eq(street => street.CityId, streetsDocument.CityId)));

                foreach (var streetChunk in streetsDocument.Streets.Chunk(500))
                {
                    context.Get<Street>().InsertMany(
                        streetChunk.Select(x => new Street()
                        {
                            Id = Guid.NewGuid(),
                            OsmId = x.Id,
                            CityId = streetsDocument.CityId,
                            CountryCode = x.CountryCode,
                            Name = x.Name,
                            NameEN = x.NameEN,
                            NameDE = x.NameDE,
                            NameRU = x.NameRU,
                            NameES = x.NameES,
                            NameFR = x.NameFR,
                            NameIT = x.NameIT,
                        }));
                }

                var buildingFile = Path.Combine($"Seeds/addresses/{countryCode}", $"{streetsDocument.CityId}-buildings.json");

                var filename = Path.GetFileName(buildingFile);

                var buildingEntities = File.ReadAllText(buildingFile).FromBuildingsDocumentJson();

                context.Get<Building>().DeleteMany(Builders<Building>.Filter.Eq(building => building.CityId, streetsDocument.CityId));

                foreach (var buildingsChunk in buildingEntities.Chunk(500))
                {
                    context.Get<Building>().InsertMany(
                        buildingsChunk.Select(x =>
                        {
                            var houseNumberFigureMatch = Regex.Match(x.HouseNumber, @"\d+");
                            var houseNumberFigure = 0;

                            if (houseNumberFigureMatch.Success)
                            {
                                houseNumberFigure = int.Parse(houseNumberFigureMatch.Value);
                            }

                            return new Building()
                            {
                                Id = Guid.NewGuid(),
                                OsmId = x.Id,
                                CityId = streetsDocument.CityId,
                                HouseNumber = x.HouseNumber,
                                HouseNumberMainComponent = houseNumberFigure,
                                Latitude = x.Latitude,
                                Longitude = x.Longitude,
                                StreetId = x.StreetId,
                            };
                        }));
                }

                Console.WriteLine($"Loading {streetsDocument.CitySearchName} elapsed: {stopwatch.ElapsedMilliseconds}");
            }
        }

        AddStreetIndexes(context);
        AddBuildingIndexes(context);
    }

    stopwatch.Stop();
}

void AddStreetIndexes(DataContext context)
{
    AddIndexes<Street>(context);

    var ascendingCityId = Builders<Street>.IndexKeys.Ascending(x => x.CityId);

    context.Get<Street>().Indexes.CreateMany(new CreateIndexModel<Street>[]
    {
        new CreateIndexModel<Street>(ascendingCityId),
    });
}

void AddBuildingIndexes(DataContext context)
{
    var ascendingCityId = Builders<Building>.IndexKeys.Ascending(x => x.CityId);
    var ascendingStreetId = Builders<Building>.IndexKeys.Ascending(x => x.StreetId);
    var ascendingOsmId = Builders<Building>.IndexKeys.Ascending(x => x.OsmId);
    var ascendingHouseNumber = Builders<Building>.IndexKeys.Ascending(x => x.HouseNumber);

    context.Get<Building>().Indexes.CreateMany(new CreateIndexModel<Building>[]
    {
        new CreateIndexModel<Building>(ascendingCityId),
        new CreateIndexModel<Building>(ascendingStreetId),
        new CreateIndexModel<Building>(ascendingOsmId),
        new CreateIndexModel<Building>(ascendingHouseNumber),
    });
}

void AddIndexes<TEntity>(DataContext context)
    where TEntity : BaseAdministrativeGeoEntity
{
    var ascendingCountryCode = Builders<TEntity>.IndexKeys.Ascending(x => x.CountryCode);
    var ascendingName = Builders<TEntity>.IndexKeys.Ascending(x => x.Name);
    var ascendingNameEN = Builders<TEntity>.IndexKeys.Ascending(x => x.NameEN);
    var ascendingNameDE = Builders<TEntity>.IndexKeys.Ascending(x => x.NameDE);
    var ascendingNameRU = Builders<TEntity>.IndexKeys.Ascending(x => x.NameRU);
    var ascendingNameES = Builders<TEntity>.IndexKeys.Ascending(x => x.NameES);
    var ascendingNameFR = Builders<TEntity>.IndexKeys.Ascending(x => x.NameFR);
    var ascendingNameIT = Builders<TEntity>.IndexKeys.Ascending(x => x.NameIT);

    context.Get<TEntity>().Indexes.CreateMany(new CreateIndexModel<TEntity>[]
    {
            new CreateIndexModel<TEntity>(ascendingName),
            new CreateIndexModel<TEntity>(ascendingNameEN),
            new CreateIndexModel<TEntity>(ascendingNameDE),
            new CreateIndexModel<TEntity>(ascendingNameRU),
            new CreateIndexModel<TEntity>(ascendingNameES),
            new CreateIndexModel<TEntity>(ascendingNameFR),
            new CreateIndexModel<TEntity>(ascendingNameIT),
    });
}