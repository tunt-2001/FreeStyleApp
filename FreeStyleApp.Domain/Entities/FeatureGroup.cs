using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreeStyleApp.Domain.Entities
{
    [Table("FeatureGroups")]
    public class FeatureGroup
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Icon { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public virtual ICollection<Feature> Features { get; set; }
        public virtual ICollection<UserFeatureGroup> UserFeatureGroups { get; set; }
    }
}

