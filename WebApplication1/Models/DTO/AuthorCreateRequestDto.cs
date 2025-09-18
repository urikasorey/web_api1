using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTO
{
    public class AuthorCreateRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
    }
}