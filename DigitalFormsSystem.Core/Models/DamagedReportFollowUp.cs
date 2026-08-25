using System;

namespace DigitalFormsSystem.Core.Models
{
    public class DamagedReportFollowUp
    {
        public int Id { get; set; }
        public int DamagedReportId { get; set; }
        public DateTime FollowUpDate { get; set; }
        public string? Status { get; set; }
        public string? UpdateBy { get; set; }
        public string? NotedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual DamagedReport? DamagedReport { get; set; }
    }
}