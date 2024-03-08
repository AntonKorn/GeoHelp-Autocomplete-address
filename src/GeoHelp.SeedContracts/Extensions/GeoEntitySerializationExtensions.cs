using System.Text.Json;

namespace GeoHelp.SeedContracts.Extensions
{
    public static class GeoEntitySerializationExtensions
    {
        public static string ToJson(this IEnumerable<GeoEntity> @this)
        {
            return JsonSerializer.Serialize(@this);
        }

        public static IEnumerable<GeoEntity> FromJsonSet(this string @this)
        {
            return JsonSerializer.Deserialize<IEnumerable<GeoEntity>>(@this)
                ?? throw new InvalidOperationException($"{nameof(@this)} should not be null");
        }
    }
}
