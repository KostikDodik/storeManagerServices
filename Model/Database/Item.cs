using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Extentions;

namespace Model.Database;

public class Item: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public DateTime UpdatedStatus { get; set; }
    [Required]
    public DateTime ReceivedDate { get; set; }
    
    [Required]
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")] 
    public Product Product { get; set; }
    
    [Required]
    public Guid SupplyId { get; set; }
    [ForeignKey("SupplyId")] 
    public Supply Supply { get; set; }
    
    public Guid? OrderId { get; set; }
    [ForeignKey("OrderId")] 
    public Order Order { get; set; }

    [Required] public ItemState State { get; set; } = ItemState.Available;
    public decimal SupplyPrice { get; set; }
    public decimal DeliveryPrice { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime? BBDate { get; set; }
}

public class AvailableItemCount: IDbEntity
{
    public Guid ProductId { get; set; }
    public ItemState State { get; set; }
    public int Count { get; set; }
}