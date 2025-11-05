using System.ComponentModel.DataAnnotations.Schema;

namespace FreeStyleApp.Models
{
    [Table("UserPermissions")]
    public class UserPermission
    {
        public string UserId { get; set; } 
        public string PermissionId { get; set; }

        public virtual User User { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
