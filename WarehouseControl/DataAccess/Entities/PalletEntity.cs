using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities;

public class PalletEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public double Width { get; set; }

    [Required]
    public double Height { get; set; }

    [Required]
    public double Depth { get; set; }

    public ICollection<BoxEntity> Boxes { get; set; } = new List<BoxEntity>();
}