using System;
using DigitalFormsSystem.Core.Models; 

namespace DigitalFormsSystem.Core.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmployeeNo { get; set; }
        public string Action { get; set; } = null!;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Employee? User { get; set; }
    }
}