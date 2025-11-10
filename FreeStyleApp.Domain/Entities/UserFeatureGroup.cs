using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreeStyleApp.Domain.Entities
{
    [Table("UserFeatureGroups")]
    public class UserFeatureGroup
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string FeatureGroupId { get; set; }
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("FeatureGroupId")]
        public virtual FeatureGroup FeatureGroup { get; set; }
    }
}

