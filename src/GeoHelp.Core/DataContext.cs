using GeoHelp.Core.Attributes;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Diagnostics;

namespace GeoHelp.Core
{
    public class DataContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;

        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var mongoConnectionUrl = new MongoUrl(_configuration.GetConnectionString("common"));
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);
            mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    Debug.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };

            _mongoClient = new MongoClient(mongoClientSettings);
            _mongoDatabase = _mongoClient.GetDatabase("GeoHelp");
        }

        public IMongoDatabase Database => _mongoDatabase;

        public IMongoCollection<T> Get<T>()
        {
            var collectionName = GetCollectionName(typeof(T));
            return _mongoDatabase.GetCollection<T>(collectionName);
        }

        private string GetCollectionName(Type type)
        {
            var attribute = type
                .GetCustomAttributes(typeof(CollectionNameAttribute), true)
                .SingleOrDefault();

            return attribute is CollectionNameAttribute collectionNameAttribute
                ? collectionNameAttribute.Name
                : type.Name;
        }
    }
}
