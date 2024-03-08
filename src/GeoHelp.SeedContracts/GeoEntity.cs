namespace GeoHelp.SeedContracts
{
    public class GeoEntity
    {
        public string Id { get; set; }

        public string CountryCode { get; set; }

        public string? Name { get; set; }

        public string? NameEN { get; set; }

        public string? NameDE { get; set; }

        public string? NameES { get; set; }

        public string? NameFR { get; set; }

        public string? NameIT { get; set; }

        public string? NameRU { get; set; }

        public GeoEntity(
            string id,
            string countryCode,
            string? name,
            string? nameEN,
            string? nameDE,
            string? nameES,
            string? nameFR,
            string? nameIT,
            string? nameRU)
        {
            Id = id;
            CountryCode = countryCode;
            Name = name;
            NameEN = nameEN;
            NameDE = nameDE;
            NameES = nameES;
            NameFR = nameFR;
            NameIT = nameIT;
            NameRU = nameRU;
        }
    }
}
