using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Extentions;

namespace Model.Database;

public class SalePlatform: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(254)]
    public string Name { get; set; }
    
    [MaxLength(254)]
    public string Code { get; set; }
    
    public decimal UsualIncrement { get; set; }
}