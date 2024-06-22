using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Extentions;

namespace Model.Database;

public class Product: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(254)]
    public string Name { get; set; }
    
    [MaxLength(254)]
    public string Code { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
    
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
}