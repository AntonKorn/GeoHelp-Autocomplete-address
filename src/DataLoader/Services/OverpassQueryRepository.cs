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

        public async Task<OsmDocument<OsmHighway>> QueryHighways(string cityName)
        {
            var result = await _osmApiService.QueryAll(
                @$"
[out:json];
area[""name""=""{cityName}""];
(
  way(area)[""highway""][""name""];
);
convert way ::id = id(), name = t[""name""], nameEn = t[""name:en""], nameDe = t[""name:de""], nameEs = t[""name:es""], nameFr = t[""name:fr""], nameIt = t[""name:it""], nameRu = t[""name:ru""];
out body;");

            if (string.IsNullOrEmpty(result))
            {
                throw new InvalidOperationException("Overpass result should not be empty");
            }

            var parsedResult = JsonConvert.DeserializeObject<OsmDocument<OsmHighway>>(result);

            if (parsedResult == null)
            {
                throw new InvalidOperationException("Oerpass result should not be null");
            }

            return parsedResult;
        }

        public async Task<OsmDocument<OsmBuilding>> QueryMultipointBuildings(string cityName)
        {
            var result = await _osmApiService.QueryAll(
                @$"
[out:json];
area[""name""=""{cityName}""];
(
  (way(area)[""building""][""addr:street""][""addr:housenumber""];);
(relation(area)[""building""][""addr:street""][""addr:housenumber""];);
(relation(area)[""building:part""][""addr:street""][""addr:housenumber""];);
);
out center;");

            if (string.IsNullOrEmpty(result))
            {
                throw new InvalidOperationException("Overpass result should not be empty");
            }

            var parsedResult = JsonConvert.DeserializeObject<OsmDocument<OsmBuilding>>(result);

            if (parsedResult == null)
            {
                throw new InvalidOperationException("Oerpass result should not be null");
            }

            return parsedResult;
        }

        public async Task<OsmDocument<OsmBuilding>> QuerySinglePointBuildings(string cityName)
        {
            var result = await _osmApiService.QueryAll(
                @$"
[out:json];
area[""name""=""{cityName}""];
node(area)[""building""][""addr:street""][""addr:housenumber""];
out body;");

            if (string.IsNullOrEmpty(result))
            {
                throw new InvalidOperationException("Overpass result should not be empty");
            }

            var parsedResult = JsonConvert.DeserializeObject<OsmDocument<OsmBuilding>>(result);

            if (parsedResult == null)
            {
                throw new InvalidOperationException("Oerpass result should not be null");
            }

            return parsedResult;
        }
    }
}
