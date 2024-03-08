using GeoHelp.Core.Entities;
using GeoHelp.Core;
using GeoHelp.SeedContracts.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using GeoHelp.Core.Extensions;
using System.Diagnostics;

var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

var serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton<IConfiguration>(configurationRoot);
serviceCollection.AddCore();

var services = serviceCollection.BuildServiceProvider();

using (var scope = services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<DataContext>() ?? throw new InvalidOperationException(nameof(DataContext));

    var countries = File.ReadAllText("Seeds/countries.json").FromJsonSet();

    context.Get<Country>().Indexes.DropAll();
    context.Get<City>().Indexes.DropAll();

    var stopwatch = new Stopwatch();
    stopwatch.Start();

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
        var cities = File.ReadAllText(cityFile).FromJsonSet();

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

void AddIndexes<TEntity>(DataContext context)
    where TEntity : BaseGeoEntity
{
    var ascendingCountryCode = Builders<TEntity>.IndexKeys.Ascending(x => x.CountryCode);
    var ascendingName = Builders<City>.IndexKeys.Ascending(x => x.Name);
    var ascendingNameEN = Builders<City>.IndexKeys.Ascending(x => x.NameEN);
    var ascendingNameDE = Builders<City>.IndexKeys.Ascending(x => x.NameDE);
    var ascendingNameRU = Builders<City>.IndexKeys.Ascending(x => x.NameRU);
    var ascendingNameES = Builders<City>.IndexKeys.Ascending(x => x.NameES);
    var ascendingNameFR = Builders<City>.IndexKeys.Ascending(x => x.NameFR);
    var ascendingNameIT = Builders<City>.IndexKeys.Ascending(x => x.NameIT);
    context.Get<City>().Indexes.CreateMany(new CreateIndexModel<City>[]
    {
            new CreateIndexModel<City>(ascendingName),
            new CreateIndexModel<City>(ascendingNameEN),
            new CreateIndexModel<City>(ascendingNameDE),
            new CreateIndexModel<City>(ascendingNameRU),
            new CreateIndexModel<City>(ascendingNameES),
            new CreateIndexModel<City>(ascendingNameFR),
            new CreateIndexModel<City>(ascendingNameIT),
    });
}