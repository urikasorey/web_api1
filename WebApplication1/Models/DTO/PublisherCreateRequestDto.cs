using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTO
{
    public class PublisherCreateRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}