using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreeStyleApp.Domain.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public string Id { get; set; } 
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<UserPermission> UserPermissions { get; set; }
    }
}
