using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Extentions;

namespace Model.Database;

public class Check: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public Guid OrderId { get; set; }
    [ForeignKey("OrderId"), JsonIgnore] 
    public Order Order { get; set; }
    
    [Required]
    public PaymentType PaymentType { get; set; }
    [Required]
    public decimal Sum { get; set; }
    public string CheckLink { get; set; }
    public string Notes { get; set; }
}

public enum PaymentType
{
    Iban,
    Card,
    NovaPay,
    RozetkaPay
}