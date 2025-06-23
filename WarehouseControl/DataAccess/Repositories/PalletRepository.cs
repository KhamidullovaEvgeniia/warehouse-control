using DataAccess.Entities;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class PalletRepository: IPalletRepository
{
    private readonly WarehouseDbContext _context;

    public PalletRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pallet?>> GetAllAsync(CancellationToken cancellationToken)
    {
        var palletEntities = await _context.Pallets
            .Include(p => p.Boxes)
            .ToListAsync(cancellationToken);
        
        var pallets = palletEntities.Select(MapToDomain).ToList();

        return pallets;
    }

    private static Pallet? MapToDomain(PalletEntity? entity)
    {
        if (entity == null) return null;

        return new Pallet
        {
            Id = entity.Id,
            Width = entity.Width,
            Height = entity.Height,
            Depth = entity.Depth,
            Boxes = entity.Boxes.Select(b => MapToDomain(b)).ToList()
        };
    }

    private static Box? MapToDomain(BoxEntity? entity)
    {
        if (entity == null) return null;

        return new Box
        {
            Id = entity.Id,
            Width = entity.Width,
            Height = entity.Height,
            Depth = entity.Depth,
            Weight = entity.Weight,
            ExpiryDate = entity.ExpiryDate,
            ManufactureDate = entity.ManufactureDate,
            PalletId = entity.PalletId
        };
    }
}