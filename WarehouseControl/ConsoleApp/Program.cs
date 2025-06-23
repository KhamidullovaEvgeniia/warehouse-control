using ConsoleApp.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataAccess;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Services.Services;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) => { config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); })
    .ConfigureServices((context, services) =>
    {
        services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
        var connectionString = context.Configuration.GetConnectionString("Warehouse");

        services.AddDataAccess(connectionString);

        services.AddScoped<IWarehouseService, WarehouseService>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var initializer = services.GetRequiredService<DbInitializer>();
await initializer.SeedAsync(services.GetRequiredService<WarehouseDbContext>());

var warehouseService = services.GetRequiredService<IWarehouseService>();
var appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;

Console.WriteLine("Группировка паллет по сроку годности:");
var grouped = await warehouseService.GroupAndSortPalletsByExpiry(default);
foreach (var group in grouped)
{
    Console.WriteLine($"\nСрок годности: {group.Key.ToShortDateString()}");
    foreach (var pallet in group)
    {
        Console.WriteLine($"  Палета {pallet.Id} — Вес: {pallet.Weight}");
    }
}

Console.WriteLine($"\nПаллет в количестве {appSettings.TopPalletCount} шт с максимальным сроком годности:");
var topPallets = await warehouseService.GetTopPalletsWithMaxBoxExpiry(appSettings.TopPalletCount, default);
foreach (var pallet in topPallets)
{
    Console.WriteLine($"Палета {pallet.Id} — Объем: {pallet.Volume}");
}