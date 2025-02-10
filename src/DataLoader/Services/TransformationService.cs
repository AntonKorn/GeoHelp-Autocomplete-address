using DataLoader.Models.Osm;
using GeoHelp.SeedContracts;

namespace DataLoader.Services
{
    public class TransformationService
    {
        public IEnumerable<AdministrativeGeoEntity> ToAdministrativeGeoEntities(OsmDocument<OsmCountry> countriesDocument)
        {
            countriesDocument.Elements = countriesDocument.Elements
                ?? throw new InvalidOperationException(nameof(countriesDocument.Elements));

            return countriesDocument.Elements.Select(e => new AdministrativeGeoEntity(
                id: e.Id ?? throw new InvalidOperationException(nameof(e.Id)),
                countryCode: e.Tags?.CountryCode ?? throw new InvalidOperationException(nameof(e.Tags.CountryCode)),
                name: e.Tags?.Name,
                nameEN: e.Tags?.NameEN,
                nameDE: e.Tags?.NameDE,
                nameES: e.Tags?.NameES,
                nameFR: e.Tags?.NameFR,
                nameIT: e.Tags?.NameIT,
                nameRU: e.Tags?.NameRU));
        }

        public IEnumerable<AdministrativeGeoEntity> ToAdministrativeGeoEntities(OsmDocument<OsmCity> countriesDocument, string countryCode)
        {
            countriesDocument.Elements = countriesDocument.Elements
                ?? throw new InvalidOperationException(nameof(countriesDocument.Elements));

            return countriesDocument.Elements.Select(e => new AdministrativeGeoEntity(
                id: e.Id ?? throw new InvalidOperationException(nameof(e.Id)),
                countryCode: countryCode,
                name: e.Tags?.Name,
                nameEN: e.Tags?.NameEN,
                nameDE: e.Tags?.NameDE,
                nameES: e.Tags?.NameES,
                nameFR: e.Tags?.NameFR,
                nameIT: e.Tags?.NameIT,
                nameRU: e.Tags?.NameRU));
        }

        public AdministrativeGeoEntity ToStreet(OsmHighway highway, string countryCode)
        {
            return new AdministrativeGeoEntity(
                id: highway.Id ?? throw new InvalidOperationException(nameof(highway.Id)),
                countryCode: countryCode,
                name: highway.Tags?.Name,
                nameEN: highway.Tags?.NameEN,
                nameDE: highway.Tags?.NameDE,
                nameES: highway.Tags?.NameES,
                nameFR: highway.Tags?.NameFR,
                nameIT: highway.Tags?.NameIT,
                nameRU: highway.Tags?.NameRU);
        }
    }
}
