using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class DbInitializer
{
    public async Task SeedAsync(WarehouseDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Pallets.AnyAsync())
            return;

        var random = new Random();

        var pallets = new List<PalletEntity>();

        for (int i = 0; i < 100; i++)
        {
            double palletWidth = random.Next(100, 150);
            double palletHeight = random.Next(10, 50);
            double palletDepth = random.Next(70, 120);
            var pallet = new PalletEntity
            {
                Width = palletWidth, Height = palletHeight, Depth = palletDepth, Boxes = new List<BoxEntity>()
            };

            int boxCount = random.Next(5, 20);

            for (int j = 0; j < boxCount; j++)
            {
                int errorWidth = 0;
                if (random.NextDouble() < 0.02)
                    errorWidth = 10;
                
                int errorDepth = 0;
                if (random.NextDouble() < 0.02)
                    errorDepth = 10;

                double boxWidth = random.Next(10, (int)pallet.Width + errorWidth);
                double boxHeight = random.Next(5, 30);
                double boxDepth = random.Next(10, (int)pallet.Depth + errorDepth);
                double weight = random.Next(1, 20);

                DateTime? manufactureDate = DateTime.Today.AddDays(-random.Next(1, 200));
                DateTime? expiryDate = null;

                if (random.NextDouble() > 0.5)
                {
                    expiryDate = manufactureDate.Value.AddDays(100);
                    manufactureDate = null;
                }

                var box = new BoxEntity
                {
                    Width = boxWidth,
                    Height = boxHeight,
                    Depth = boxDepth,
                    Weight = weight,
                    ManufactureDate = manufactureDate,
                    ExpiryDate = expiryDate,
                    Pallet = pallet
                };

                pallet.Boxes.Add(box);
            }

            pallets.Add(pallet);
        }

        context.Pallets.AddRange(pallets);
        await context.SaveChangesAsync();
    }
}