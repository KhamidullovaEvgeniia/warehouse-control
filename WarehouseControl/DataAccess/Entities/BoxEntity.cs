using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

public class BoxEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public double Width { get; set; }

    [Required]
    public double Height { get; set; }

    [Required]
    public double Depth { get; set; }

    [Required]
    public double Weight { get; set; }

    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [ForeignKey("Pallet")]
    public Guid PalletId { get; set; }

    public PalletEntity Pallet { get; set; }
}