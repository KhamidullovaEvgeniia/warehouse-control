using DataAccess.Configurations;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class WarehouseDbContext : DbContext
{
    public DbSet<PalletEntity> Pallets { get; set; }
    public DbSet<BoxEntity> Boxes { get; set; }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PalletConfiguration());
        modelBuilder.ApplyConfiguration(new BoxConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}