using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreeStyleApp.Domain.Entities
{
    [Table("Features")]
    public class Feature
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        
        [MaxLength(200)]
        public string? Controller { get; set; }
        
        [MaxLength(200)]
        public string? Action { get; set; }
        
        [MaxLength(50)]
        public string? Icon { get; set; }
        
        [Required]
        public string FeatureGroupId { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [ForeignKey("FeatureGroupId")]
        public virtual FeatureGroup FeatureGroup { get; set; }
    }
}

