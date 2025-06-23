namespace Domain.Models;

public class Box
{
    private const int _storageLife = 100;

    public Guid Id { get; init; } = Guid.NewGuid();

    public double Width { get; init; }

    public double Height { get; init; }

    public double Depth { get; init; }

    public double Weight { get; init; }

    public DateTime? ExpiryDate { get; init; }

    public DateTime? ManufactureDate { get; init; }

    public Guid PalletId { get; set; }

    public double Volume => Width * Height * Depth;

    public DateTime CalculatedExpiryDate =>
        ExpiryDate
        ?? ManufactureDate?.AddDays(_storageLife)
        ?? throw new InvalidOperationException("У коробки должен быть указан срок годности или дата производства");
}