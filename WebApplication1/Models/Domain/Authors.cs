using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Domain
{
    public class Author
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        //Navigation Properties - One author has many book_author
        public List<Book_Author> Book_Authors { get; set; }
    }
}