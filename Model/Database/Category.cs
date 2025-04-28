using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Extentions;

namespace Model.Database;

public class Category: IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(254)]
    public string Name { get; set; }
    
    [MaxLength(254)]
    public string Code { get; set; }
    
    //The foreign key
    public Guid? ParentId { get; set; }
    // Navigation property
    [ForeignKey("ParentId"), JsonIgnore]
    public Category Parent { get; set; }
    
    public virtual ICollection<Category> Children { get; set; }
    public virtual ICollection<CommissionCategory> CommissionCategories { get; set; }
}