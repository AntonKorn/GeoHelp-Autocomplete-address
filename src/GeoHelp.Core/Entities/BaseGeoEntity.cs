using MongoDB.Bson.Serialization.Attributes;

namespace GeoHelp.Core.Entities
{
    public class BaseGeoEntity
    {
        [BsonId]
        public Guid Id { get; set; }

        public string? OsmId { get; set; }

        public string? CountryCode { get; set; }

        public string? Name { get; set; }

        public string? NameEN { get; set; }

        public string? NameDE { get; set; }

        public string? NameES { get; set; }

        public string? NameFR { get; set; }

        public string? NameIT { get; set; }

        public string? NameRU { get; set; }
    }
}
