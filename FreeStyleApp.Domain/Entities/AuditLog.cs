using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreeStyleApp.Domain.Entities
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string ActionType { get; set; }
        public string Details { get; set; }
    }
}