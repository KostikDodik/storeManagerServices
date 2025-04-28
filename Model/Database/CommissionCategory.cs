using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Extentions;

namespace Model.Database;

public class CommissionCategory: IDbEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }
    [Required]
    public Guid SalePlatformId { get; set; }
    [ForeignKey("SalePlatformId"), JsonIgnore] 
    public SalePlatform SalePlatform { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    [ForeignKey("CategoryId"), JsonIgnore] 
    public Category Category { get; set; }
    public virtual ICollection<CommissionSize> CommissionSizes { get; set; }
}