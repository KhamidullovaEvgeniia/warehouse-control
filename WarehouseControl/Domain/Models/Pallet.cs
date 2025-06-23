using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class Pallet
{
    private const int _palletWeight = 30;

    public Guid Id { get; init; } = Guid.NewGuid();

    public double Width { get; init; }

    public double Height { get; init; }

    public double Depth { get; init; }

    public List<Box?> Boxes { get; init; } = new();

    [NotMapped]
    public double Weight => Boxes.Sum(b => b.Weight) + _palletWeight;

    [NotMapped]
    public double Volume => Width * Height * Depth + Boxes.Sum(b => b.Volume);

    [NotMapped]
    public DateTime? ExpiryDate =>
        Boxes.Count == 0 ? null : Boxes.Min(b => b.CalculatedExpiryDate);
}