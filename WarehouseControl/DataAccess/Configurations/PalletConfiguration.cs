using DataAccess.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class PalletConfiguration : IEntityTypeConfiguration<PalletEntity>
{
    public void Configure(EntityTypeBuilder<PalletEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Width).IsRequired();
        builder.Property(p => p.Height).IsRequired();
        builder.Property(p => p.Depth).IsRequired();

        builder.HasMany(p => p.Boxes)
            .WithOne(b => b.Pallet)
            .HasForeignKey(b => b.PalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}