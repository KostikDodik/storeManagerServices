using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Extentions;

namespace Model.Database;

public class Supply: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public DateTime UpdatedState { get; set; }
    [Required]
    public DateTime DateEdited { get; set; }
    
    public string TrackingNumber { get; set; }
    public string Name { get; set; }
    
    [Required]
    public Guid SupplierId { get; set; }
    [ForeignKey("SupplierId"), JsonIgnore]
    public Supplier Supplier { get; set; }
    [Required]
    public int Number { get; set; }
    public virtual ICollection<SupplyRow> Rows { get; set; }
    [JsonIgnore]
    public virtual ICollection<Item> Items { get; set; }

    public SupplyState State { get; set; } = SupplyState.Paid;
    public decimal DeliveryFee { get; set; }
}