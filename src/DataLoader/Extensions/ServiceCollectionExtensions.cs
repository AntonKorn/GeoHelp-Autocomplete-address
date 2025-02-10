using DataLoader.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DataLoader.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDataLoader(this IServiceCollection services)
        {
            services.AddSingleton<OverpassApiService>();
            services.AddSingleton<OverpassQueryRepository>();
            services.AddSingleton<TransformationService>();
        }
    }
}
