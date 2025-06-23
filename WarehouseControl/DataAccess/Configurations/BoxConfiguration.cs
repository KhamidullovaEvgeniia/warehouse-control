using DataAccess.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class BoxConfiguration : IEntityTypeConfiguration<BoxEntity>
{
    public void Configure(EntityTypeBuilder<BoxEntity> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Width).IsRequired();
        builder.Property(b => b.Height).IsRequired();
        builder.Property(b => b.Depth).IsRequired();
        builder.Property(b => b.Weight).IsRequired();

        builder.Property(b => b.ExpiryDate).HasColumnType("date");
        
        builder.Property(b => b.ManufactureDate).HasColumnType("date");
    }
}