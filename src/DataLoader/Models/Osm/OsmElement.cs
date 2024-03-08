namespace DataLoader.Models.Osm
{
    public class OsmElement<T>
    {
        public string? Id { get; set; }

        public T? Tags { get; set; }
    }
}
