using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTO
{
    public class BookCreateRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public bool IsRead { get; set; }
        
        public DateTime? DateRead { get; set; }
        
        [Range(1, 5)]
        public int? Rate { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Genre { get; set; }
        
        public string? CoverUrl { get; set; }
        
        [Required]
        public int PublisherID { get; set; }
        
        public List<int> AuthorIds { get; set; } = new List<int>();
    }
}