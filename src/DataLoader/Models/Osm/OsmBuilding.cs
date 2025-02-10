namespace DataLoader.Models.Osm
{
    public class OsmBuilding : OsmElement<Dictionary<string, string>>
    {
        public string? HouseNumber
        {
            get
            {
                if (Tags is null)
                {
                    return null;
                }

                Tags.TryGetValue("addr:housenumber", out var houseNumber);

                return houseNumber;
            }
        }

        public string? Street
        {
            get
            {
                if (Tags is null)
                {
                    return null;
                }

                Tags.TryGetValue("addr:street", out var street);

                return street;
            }
        }
    }
}
