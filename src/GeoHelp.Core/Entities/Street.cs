namespace GeoHelp.Core.Entities
{
    public class Street : BaseAdministrativeGeoEntity
    {
        public string? CityId { get; set; }

        public IEnumerable<Building>? Buildings { get; set; }
    }
}
