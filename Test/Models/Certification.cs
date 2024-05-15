using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Test.Models
{
    public class Certification
    {
        [Key]
        public int CertificationId { get; set; }

        [Required]
        [Display(Name = "Số GCN")]
        public string? CertificateNumber { get; set; }

        [Required]
        [Display(Name = "Mã trạm")]
        public string? StationCode { get; set; }

        [Required]
        [Display(Name = "Ngày cấp GCN")]
        [DataType(DataType.Date)]
        public DateTime InspectionDate { get; set; }

        [Display(Name = "Đợt")]
        public string? Note { get; set; }

        [Required]
        [Display(Name = "Năm")]
        public int InspectionYear { get; set; }

        [Required]
        [Display(Name = "Tỉnh")]
        public string? ProvinceName { get; set; }

        [Required]
        [Display(Name = "Ngày gửi")]
        public DateTime SentTime { get; set; }

        // Đường dẫn đến file PDF liên quan đến Certification
        [Display(Name = "Trạng thái")]
        public string? FilePath { get; set; }
    }

    
}
