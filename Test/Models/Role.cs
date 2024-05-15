using System.ComponentModel.DataAnnotations;

namespace Test.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        [Display(Name = "Vai trò")]
        public string? RoleName { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}
