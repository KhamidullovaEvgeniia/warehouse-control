using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Services;

namespace Services.Tests;

public class WarehouseServiceTests
{
    private readonly Mock<IPalletRepository> _mockRepo = new();

    private readonly Mock<ILogger<WarehouseService>> _mockLogger;

    public WarehouseServiceTests()
    {
        _mockLogger = new Mock<ILogger<WarehouseService>>();
    }

    private WarehouseService CreateService(List<Pallet> pallets)
    {
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pallets);
        return new WarehouseService(_mockRepo.Object, _mockLogger.Object);
    }

    private Pallet CreatePallet(double width, double depth, double height, params Box[] boxes)
    {
        return new Pallet { Width = width, Depth = depth, Height = height, Boxes = boxes.ToList() };
    }

    private Box CreateBox(
        DateTime? expiryDate = null,
        DateTime? manufactureDate = null,
        double width = 1,
        double depth = 1,
        double height = 1,
        double weight = 1)
    {
        return new Box
        {
            Width = width,
            Depth = depth,
            Height = height,
            Weight = weight,
            ExpiryDate = expiryDate,
            ManufactureDate = manufactureDate
        };
    }

    [Fact]
    public async Task GroupAndSortPalletsByExpiry_ShouldGroupAndSortCorrectly()
    {
        var pallets = new List<Pallet>
        {
            CreatePallet(10, 10, 10, CreateBox(expiryDate: new DateTime(2025, 1, 1), weight: 2)),
            CreatePallet(10, 10, 10, CreateBox(expiryDate: new DateTime(2025, 1, 1), weight: 1)),
            CreatePallet(10, 10, 10, CreateBox(expiryDate: new DateTime(2024, 12, 31), weight: 3))
        };

        var service = CreateService(pallets);

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        var groups = result.ToList();
        Assert.Equal(2, groups.Count);
        Assert.Equal(new DateTime(2024, 12, 31), groups[0].Key);
        Assert.Single(groups[0]);
        Assert.Equal(new DateTime(2025, 1, 1), groups[1].Key);
        Assert.Equal(2, groups[1].Count());
    }

    [Fact]
    public async Task GetTopPalletsWithMaxBoxExpiry_ShouldReturnTopByBoxExpiryThenSortByVolume()
    {
        var pallet1 = CreatePallet(10, 10, 10, CreateBox(expiryDate: new DateTime(2025, 1, 1)));
        var pallet2 = CreatePallet(5, 5, 5, CreateBox(expiryDate: new DateTime(2026, 1, 1)));
        var pallet3 = CreatePallet(10, 10, 10, CreateBox(expiryDate: new DateTime(2023, 1, 1)));

        var pallets = new List<Pallet> { pallet1, pallet2, pallet3 };
        var service = CreateService(pallets);

        var result = await service.GetTopPalletsWithMaxBoxExpiry(2, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(pallet2.Id, result[0].Id);
        Assert.Equal(pallet1.Id, result[1].Id);
    }

    [Fact]
    public async Task ValidatePallets_ShouldExcludeInvalidPalletsAndBoxes()
    {
        var badPallet = CreatePallet(10, 10, 0, CreateBox(expiryDate: DateTime.Today));

        var badBox = CreateBox(expiryDate: DateTime.Today, width: 20, depth: 20);
        var palletWithBadBox = CreatePallet(10, 10, 10, badBox);

        var goodPallet = CreatePallet(10, 10, 10, CreateBox(expiryDate: DateTime.Today));

        var pallets = new List<Pallet> { badPallet, palletWithBadBox, goodPallet };
        var service = CreateService(pallets);

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Single(result.SelectMany(g => g));
    }

    [Fact]
    public async Task GroupAndSortPalletsByExpiry_ShouldReturnEmpty_WhenNoValidData()
    {
        var pallets = new List<Pallet> { CreatePallet(0, 0, 0, CreateBox(expiryDate: DateTime.Today)) };

        var service = CreateService(pallets);

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task BoxWithNoDates_ShouldBeExcluded()
    {
        var invalidBox = CreateBox();
        var pallet = CreatePallet(10, 10, 10, invalidBox);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task BoxWiderThanPallet_ShouldBeExcluded()
    {
        var invalidBox = CreateBox(expiryDate: DateTime.Today, width: 15, depth: 5);
        var pallet = CreatePallet(10, 10, 10, invalidBox);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task BoxDeeperThanPallet_ShouldBeExcluded()
    {
        var invalidBox = CreateBox(expiryDate: DateTime.Today, width: 5, depth: 15);
        var pallet = CreatePallet(10, 10, 10, invalidBox);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Pallet_WithZeroWidth_ShouldBeExcluded()
    {
        var pallet = CreatePallet(0, 10, 10, CreateBox(expiryDate: DateTime.Today));
        var service = CreateService(new List<Pallet> { pallet });

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Pallet_WithNegativeWidth_ShouldBeExcluded()
    {
        var pallet = CreatePallet(-1, 10, 10, CreateBox(expiryDate: DateTime.Today));
        var service = CreateService(new List<Pallet> { pallet });

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Pallet_WithZeroDepth_ShouldBeExcluded()
    {
        var pallet = CreatePallet(10, 0, 10, CreateBox(expiryDate: DateTime.Today));
        var service = CreateService(new List<Pallet> { pallet });

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Pallet_WithNegativeDepth_ShouldBeExcluded()
    {
        var pallet = CreatePallet(10, -1, 10, CreateBox(expiryDate: DateTime.Today));
        var service = CreateService(new List<Pallet> { pallet });

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Pallet_WithZeroHeight_ShouldBeExcluded()
    {
        var pallet = CreatePallet(10, 10, 0, CreateBox(expiryDate: DateTime.Today));
        var service = CreateService(new List<Pallet> { pallet });

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Pallet_WithNegativeHeight_ShouldBeExcluded()
    {
        var pallet = CreatePallet(10, 10, -1, CreateBox(expiryDate: DateTime.Today));
        var service = CreateService(new List<Pallet> { pallet });

        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Box_WithZeroWidth_ShouldBeExcluded()
    {
        var box = CreateBox(expiryDate: DateTime.Today, width: 0);
        var pallet = CreatePallet(10, 10, 10, box);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Box_WithNegativeWidth_ShouldBeExcluded()
    {
        var box = CreateBox(expiryDate: DateTime.Today, width: -1);
        var pallet = CreatePallet(10, 10, 10, box);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Box_WithZeroDepth_ShouldBeExcluded()
    {
        var box = CreateBox(expiryDate: DateTime.Today, depth: 0);
        var pallet = CreatePallet(10, 10, 10, box);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Box_WithNegativeDepth_ShouldBeExcluded()
    {
        var box = CreateBox(expiryDate: DateTime.Today, depth: -1);
        var pallet = CreatePallet(10, 10, 10, box);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Box_WithZeroHeight_ShouldBeExcluded()
    {
        var box = CreateBox(expiryDate: DateTime.Today, height: 0);
        var pallet = CreatePallet(10, 10, 10, box);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Box_WithNegativeHeight_ShouldBeExcluded()
    {
        var box = CreateBox(expiryDate: DateTime.Today, height: -1);
        var pallet = CreatePallet(10, 10, 10, box);

        var service = CreateService(new List<Pallet> { pallet });
        var result = await service.GroupAndSortPalletsByExpiry(CancellationToken.None);

        Assert.Empty(result);
    }
}