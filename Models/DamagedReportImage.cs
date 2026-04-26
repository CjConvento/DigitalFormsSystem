using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalFormsSystem.Models
{
    public class DamagedReportImage
    {
        [Key]
        public int Id { get; set; }

        public int DamagedReportId { get; set; }

        [Required]
        [StringLength(20)]
        public string Section { get; set; } = null!; // "PartI" or "PartII"

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = null!;

        public int DisplayOrder { get; set; } = 0;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [ForeignKey("DamagedReportId")]
        public virtual DamagedReport? DamagedReport { get; set; }
    }
}