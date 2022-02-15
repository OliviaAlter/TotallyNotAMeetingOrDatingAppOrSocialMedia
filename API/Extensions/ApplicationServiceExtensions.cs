using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenServices, TokenServices>();
        services.AddDbContext<DataContext>(options =>
        {
            options.UseMySql(configuration.GetConnectionString("TotallyNotAConnectionStringOverHere"),
                new MySqlServerVersion(new Version(8, 0, 27)));
        });
        return services;
    }
}