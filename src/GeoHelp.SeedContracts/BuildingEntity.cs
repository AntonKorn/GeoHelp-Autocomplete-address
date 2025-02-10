namespace GeoHelp.SeedContracts
{
    public class BuildingEntity
    {
        public string Id { get; set; }

        public string StreetId { get; set; }

        public string HouseNumber { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public BuildingEntity(
            string id,
            string streetId,
            string houseNumber,
            decimal latitude,
            decimal longitude)
        {
            Id = id;
            StreetId = streetId;
            HouseNumber = houseNumber;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
