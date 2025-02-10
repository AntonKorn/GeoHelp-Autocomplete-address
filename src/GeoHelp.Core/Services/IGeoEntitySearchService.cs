using GeoHelp.Core.Entities;

namespace GeoHelp.Core.Services
{
    public interface IGeoEntitySearchService
    {
        IQueryable<BaseAdministrativeGeoEntity> Query(
            IQueryable<BaseAdministrativeGeoEntity> baseQuery,
            int skip,
            int take,
            string[] searchLocales,
            string query);
    }
}
