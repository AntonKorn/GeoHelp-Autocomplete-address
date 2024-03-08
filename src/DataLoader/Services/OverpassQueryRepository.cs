using DataLoader.Models.Osm;
using Newtonsoft.Json;

namespace DataLoader.Services
{
    public class OverpassQueryRepository
    {
        private readonly OverpassApiService _osmApiService;

        public OverpassQueryRepository(OverpassApiService osmApiService)
        {
            _osmApiService = osmApiService;
        }

        public async Task<OsmDocument<OsmCountry>> QueryCoutries()
        {
            var result = await _osmApiService.QueryAll(
                @"
[out:json];relation[""admin_level""=""2""][""ISO3166-1""][boundary=administrative];
convert relation ::id = id(), countryCode = t[""ISO3166-1""], name = t[""name""], nameEn = t[""name:en""], nameDe = t[""name:de""], nameEs = t[""name:es""], nameFr = t[""name:fr""], nameIt = t[""name:it""], nameRu = t[""name:ru""];
out tags;");

            if (string.IsNullOrEmpty(result))
            {
                throw new InvalidOperationException("Overpass result should not be empty");
            }

            var parsedResult = JsonConvert.DeserializeObject<OsmDocument<OsmCountry>>(result);

            if (parsedResult == null)
            {
                throw new InvalidOperationException("Oerpass result should not be null");
            }

            return parsedResult;
        }

        public async Task<OsmDocument<OsmCity>> QueryCities(string countryCode)
        {
            var result = await _osmApiService.QueryAll(
                @$"
[out:json];area[""admin_level""=""2""][""ISO3166-1""=""{countryCode}""]->.searchArea;
node[""place""~""city|town""](area.searchArea);
convert relation ::id = id(), name = t[""name""], nameEn = t[""name:en""], nameDe = t[""name:de""], nameEs = t[""name:es""], nameFr = t[""name:fr""], nameIt = t[""name:it""], nameRu = t[""name:ru""];
out tags;");

            if (string.IsNullOrEmpty(result))
            {
                throw new InvalidOperationException("Overpass result should not be empty");
            }

            var parsedResult = JsonConvert.DeserializeObject<OsmDocument<OsmCity>>(result);

            if (parsedResult == null)
            {
                throw new InvalidOperationException("Oerpass result should not be null");
            }

            return parsedResult;
        }
    }
}
