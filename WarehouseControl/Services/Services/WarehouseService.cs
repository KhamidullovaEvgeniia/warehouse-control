using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Services.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IPalletRepository _palletRepository;

    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(IPalletRepository palletRepository, ILogger<WarehouseService> logger)
    {
        _palletRepository = palletRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<IGrouping<DateTime, Pallet>>> GroupAndSortPalletsByExpiry(CancellationToken cancellationToken)
    {
        var pallets = await _palletRepository.GetAllAsync(cancellationToken);

        var validPallets = ValidatePallets(pallets.ToList());

        return validPallets
            .Where(p => p.ExpiryDate.HasValue)
            .OrderBy(p => p.ExpiryDate.Value)
            .ThenBy(p => p.Weight)
            .GroupBy(p => p.ExpiryDate.Value)
            .OrderBy(g => g.Key);
    }

    public async Task<List<Pallet>> GetTopPalletsWithMaxBoxExpiry(int countTopPallet, CancellationToken cancellationToken)
    {
        var pallets = await _palletRepository.GetAllAsync(cancellationToken);

        var validPallets = ValidatePallets(pallets.ToList());
        return validPallets
            .Where(p => p.Boxes.Any())
            .OrderByDescending(p => p.Boxes.Max(b => b.CalculatedExpiryDate))
            .Take(countTopPallet)
            .OrderBy(p => p.Volume)
            .ToList();
    }

    private List<Pallet> ValidatePallets(List<Pallet> pallets)
    {
        List<Pallet> validPallets = new List<Pallet>();
        foreach (var pallet in pallets)
        {
            if (pallet.Width <= 0)
            {
                _logger.LogError("Паллета {Id} имеет недопустимую ширину: {Width}", pallet.Id, pallet.Width);
                continue;
            }
            if (pallet.Depth <= 0)
            {
                _logger.LogError("Паллета {Id} имеет недопустимую глубину: {Depth}", pallet.Id, pallet.Depth);
                continue;
            }
            if (pallet.Height <= 0)
            {
                _logger.LogError("Паллета {Id} имеет недопустимую высоту: {Height}", pallet.Id, pallet.Height);
                continue;
            }

            var isValidBoxes = true;
            foreach (var box in pallet.Boxes)
            {
                var isValidBox = ValidateBox(box, pallet);
                if (isValidBox)
                    continue;

                isValidBoxes = false;
                break;
            }

            if (isValidBoxes)
                validPallets.Add(pallet);
        }
        return validPallets;
    }

    private bool ValidateBox(Box box, Pallet pallet)
    {
        if (box.Width <= 0)
        {
            _logger.LogError("Коробка {Id} имеет недопустимую ширину: {Width}", box.Id, box.Width);
            return false;
        }
        if (box.Depth <= 0)
        {
            _logger.LogError("Коробка {Id} имеет недопустимую глубину: {Depth}", box.Id, box.Depth);
            return false;
        }
        if (box.Height <= 0)
        {
            _logger.LogError("Коробка {Id} имеет недопустимую высоту: {Height}", box.Id, box.Height);
            return false;
        }

        if (box.Width > pallet.Width)
        {
            _logger.LogError("Коробка {BoxId} шириной {BoxWidth} не помещается в паллет {PalletId} шириной {PalletWidth}", box.Id, box.Width, pallet.Id, pallet.Width);
            return false;
        }
        if (box.Depth > pallet.Depth)
        {
            _logger.LogError("Коробка {BoxId} глубиной {BoxDepth} не помещается в паллет {PalletId} глубиной {PalletDepth}", box.Id, box.Depth, pallet.Id, pallet.Depth);
            return false;
        }

        if (!box.ExpiryDate.HasValue && !box.ManufactureDate.HasValue)
        {
            _logger.LogError("У коробки {Id} отсутствует срок годности и дата производства", box.Id);
            return false;
        }

        return true;
    }
}