using GeoHelp.Core.Entities;
using System.Linq.Expressions;

namespace GeoHelp.Core.Services.Impl
{
    internal class GeoEntitySearchService : IGeoEntitySearchService
    {
        public IQueryable<BaseGeoEntity> Query(
            IQueryable<BaseGeoEntity> baseQuery,
            int skip,
            int take,
            string[] searchLocales,
            string searchTerm)
        {
            var resultQuery = baseQuery;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var predicate = GetExpression(searchLocales, searchTerm);
                resultQuery = resultQuery.Where(predicate);
            }

            resultQuery = resultQuery
                .Skip(skip)
                .Take(take);

            return resultQuery;
        }

        private Expression<Func<BaseGeoEntity, bool>> GetExpression(string[] nameLocales, string query)
        {
            var parameter = Expression.Parameter(typeof(BaseGeoEntity), "x");
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })
                ?? throw new InvalidOperationException("Contains method not found");

            var availableFieldNames = typeof(BaseGeoEntity)
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Select(x => x.Name)
                .Where(x => x.StartsWith("Name"))
                .ToHashSet();

            Expression predicateBody = null;

            var fieldNames = nameLocales
                .Select(locale => $"Name{locale.ToUpperInvariant()}")
                .Where(x => availableFieldNames.Contains(x));

            if (!fieldNames.Any())
            {
                fieldNames = availableFieldNames;
            }
            else
            {
                fieldNames = fieldNames.Append("Name");
            }

            foreach (var fieldName in fieldNames)
            {
                var property = Expression.Property(parameter, fieldName);
                var containsCall = Expression.Call(property, containsMethod, Expression.Constant(query));

                predicateBody = predicateBody == null
                    ? containsCall
                    : Expression.Or(predicateBody, containsCall);
            }

            var predicate = Expression.Lambda<Func<BaseGeoEntity, bool>>(
                predicateBody ?? throw new InvalidOperationException("Predicate should not be empty"),
                parameter);

            return predicate;
        }
    }
}
