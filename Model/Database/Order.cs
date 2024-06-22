using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Extentions;

namespace Model.Database;

public class Order: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public DateTime UpdatedState { get; set; }
    public string TrackingNumber { get; set; }
    [Required]
    public Guid SalePlatformId { get; set; }
    [ForeignKey("SalePlatformId")]
    public SalePlatform SalePlatform { get; set; }

    public ItemState State { get; set; } = ItemState.Ordered;
}