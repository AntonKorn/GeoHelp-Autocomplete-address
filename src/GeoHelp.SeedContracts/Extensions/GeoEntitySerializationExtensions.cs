using System.Text.Json;

namespace GeoHelp.SeedContracts.Extensions
{
    public static class GeoEntitySerializationExtensions
    {
        public static string ToJson(this IEnumerable<AdministrativeGeoEntity> @this)
        {
            return JsonSerializer.Serialize(@this);
        }

        public static string ToJson(this StreetsDocument @this)
        {
            return JsonSerializer.Serialize(@this);
        }

        public static string ToJson(this IEnumerable<BuildingEntity> @this)
        {
            return JsonSerializer.Serialize(@this);
        }

        public static IEnumerable<BuildingEntity> FromBuildingsDocumentJson(this string @this)
        {
            return JsonSerializer.Deserialize<IEnumerable<BuildingEntity>>(@this)
                ?? throw new InvalidOperationException($"{nameof(@this)} should not be null");
        }

        public static StreetsDocument FromStreetsDocumentJson(this string @this)
        {
            return JsonSerializer.Deserialize<StreetsDocument>(@this)
                ?? throw new InvalidOperationException($"{nameof(@this)} should not be null");
        }

        public static IEnumerable<AdministrativeGeoEntity> FromAdministrativeJsonSet(this string @this)
        {
            return JsonSerializer.Deserialize<IEnumerable<AdministrativeGeoEntity>>(@this)
                ?? throw new InvalidOperationException($"{nameof(@this)} should not be null");
        }
    }
}
