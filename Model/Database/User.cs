using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Database;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Username { get; set; }
    public byte[] Hash { get; set; }
    public byte[] Salt { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public virtual ICollection<UserRole> Roles { get; set; }
}