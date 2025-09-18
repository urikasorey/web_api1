namespace WebApplication1.Models.DTO
{
    public class BookResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public int? Rate { get; set; }
        public string Genre { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime DateAdded { get; set; }
        
        // Publisher Information
        public int PublisherID { get; set; }
        public string PublisherName { get; set; }
        
        // Authors Information
        public List<AuthorResponseDto> Authors { get; set; } = new List<AuthorResponseDto>();
    }
}