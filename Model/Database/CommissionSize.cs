using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Extentions;

namespace Model.Database;

public class CommissionSize: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public Guid CommissionCategoryId { get; set; }
    [ForeignKey("CommissionCategoryId"), JsonIgnore] 
    public CommissionCategory CommissionCategory { get; set; }
    /// <summary>
    /// Commission size in percents
    /// </summary>
    [Required]
    public decimal Commission { get; set; }
    /// <summary>
    /// Starts working for prices over
    /// </summary>
    public decimal PriceOver { get; set; }
}