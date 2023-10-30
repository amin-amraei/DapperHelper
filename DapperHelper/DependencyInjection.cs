using DapperHelper.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DapperHelper
{
    public static class DependencyInjection
    {
        public static void AddDapper(this IServiceCollection services, string connectionStringName)
        {
            services.AddScoped<IDapper, Dapper>(
                serviceProvider => new Dapper(
                    config: serviceProvider.GetRequiredService<IConfiguration>(),
                    connectionStringName: connectionStringName));            
        }
    }
}