using MongoDB.Bson.Serialization.Attributes;

namespace GeoHelp.Core.Entities
{
    public class Building
    {
        [BsonId]
        public Guid Id { get; set; }

        public string? CityId { get; set; }

        public string? OsmId { get; set; }

        public string? StreetId { get; set; }

        public string? HouseNumber { get; set; }

        public int HouseNumberMainComponent { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
    }
}
