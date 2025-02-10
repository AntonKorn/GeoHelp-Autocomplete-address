namespace GeoHelp.SeedContracts
{
    public class StreetsDocument
    {
        public StreetsDocument(string cityId, string citySearchName, IEnumerable<AdministrativeGeoEntity> streets)
        {
            CityId = cityId;
            CitySearchName = citySearchName;
            Streets = streets;
        }

        public string CityId { get; set; }

        public string CitySearchName { get; set; }

        public IEnumerable<AdministrativeGeoEntity> Streets { get; set; }
    }
}
