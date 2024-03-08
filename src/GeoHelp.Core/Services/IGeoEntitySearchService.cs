using GeoHelp.Core.Entities;

namespace GeoHelp.Core.Services
{
    public interface IGeoEntitySearchService
    {
        IQueryable<BaseGeoEntity> Query(
            IQueryable<BaseGeoEntity> baseQuery,
            int skip,
            int take,
            string[] searchLocales,
            string query);
    }
}
