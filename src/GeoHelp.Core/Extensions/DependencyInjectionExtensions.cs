using GeoHelp.Core.Services;
using GeoHelp.Core.Services.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace GeoHelp.Core.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void  AddCore(this IServiceCollection @this)
        {
            @this.AddScoped<IGeoEntitySearchService, GeoEntitySearchService>();
            @this.AddScoped<DataContext>();
        }
    }
}
