using DataAccess.Repositories;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class Bootstrapper
{
     public static IServiceCollection AddDataAccess(this IServiceCollection services, string? connectionString)
     {
         if(connectionString is null)
             throw new ArgumentNullException(nameof(connectionString));
         
         services.AddDbContext<WarehouseDbContext>(options =>
             options.UseSqlite(connectionString));

         services.AddScoped<IPalletRepository, PalletRepository>();
         
         services.AddScoped<DbInitializer>();

         return services;
     }
}