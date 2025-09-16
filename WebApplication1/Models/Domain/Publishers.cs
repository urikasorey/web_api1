using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Domain
{
    public class Publisher
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        //Navigation Properties - One publisher has many books
        public List<Book> Books { get; set; }
    }
}