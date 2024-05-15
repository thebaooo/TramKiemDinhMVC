using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Test.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        [Display(Name = "Tên đăng nhập")]
        public string? UserName { get; set; }
        [Required]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }
        [Required]
        [Display(Name = "Tên đầy đủ")]
        public string? FullName { get; set; }
        [Required]
        [Display(Name = "Tỉnh")]
        public string? ProvinceName { get; set; }
        [ForeignKey("RoleId")]
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public bool KeepLoggedIn { get; set; }
    }
}
