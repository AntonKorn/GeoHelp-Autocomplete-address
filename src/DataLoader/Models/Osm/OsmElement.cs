namespace DataLoader.Models.Osm
{
    public class OsmElement<T>
    {
        public string? Id { get; set; }

        public T? Tags { get; set; }

        public CenterSubelement? Center { get; set; }

        public decimal? Lat { get; set; }

        public decimal? Lon { get; set; }
    }
}
