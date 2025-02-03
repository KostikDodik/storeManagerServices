using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Database;
public class UserRole
{
    [Key, Required]
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    [Key, Required]
    public Guid RoleId { get; set; }
    [ForeignKey("RoleId")]
    public Role Role { get; set; }
}