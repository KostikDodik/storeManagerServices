using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Extentions;

namespace Model.Database;

public class SupplyRow: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")] 
    public Product Product { get; set; }
    public decimal SupplyPrice { get; set; }
    public decimal DeliveryPrice { get; set; }
    
    [Required]
    public Guid SupplyId { get; set; }
    [ForeignKey("SupplyId"), JsonIgnore] 
    public Supply Supply { get; set; }
    
    [Required]
    public int Count { get; set; }
}