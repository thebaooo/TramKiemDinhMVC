using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Models
{
    public class PdfFile
    {
        [Key]
        public int PdfFileId { get; set; }

        [Required]
        [Display(Name = "Tên file")]
        public string? FileName { get; set; }

        [Required]
        [Display(Name = "Đường dẫn")]
        public string? FilePath { get; set; }

        [Display(Name = "Ngày tải lên")]
        public DateTime UploadDateTime { get; set; }
    }
}
