using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Domain;
using WebApplication1.Models.DTO;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookResponseDto>>> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                    .ThenInclude(ba => ba.Author)
                .ToListAsync();

            var bookDtos = books.Select(book => new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.DateRead,
                Rate = book.Rate,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherID = book.PublisherID,
                PublisherName = book.Publisher.Name,
                Authors = book.Book_Authors.Select(ba => new AuthorResponseDto
                {
                    Id = ba.Author.Id,
                    FullName = ba.Author.FullName
                }).ToList()
            }).ToList();

            return Ok(bookDtos);
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookResponseDto>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound($"Book with ID {id} was not found.");
            }

            var bookDto = new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.DateRead,
                Rate = book.Rate,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherID = book.PublisherID,
                PublisherName = book.Publisher.Name,
                Authors = book.Book_Authors.Select(ba => new AuthorResponseDto
                {
                    Id = ba.Author.Id,
                    FullName = ba.Author.FullName
                }).ToList()
            };

            return Ok(bookDto);
        }

        // POST: api/Books
        [HttpPost]
        public async Task<ActionResult<BookResponseDto>> CreateBook(BookCreateRequestDto bookDto)
        {
            // Validate publisher exists
            var publisher = await _context.Publishers.FindAsync(bookDto.PublisherID);
            if (publisher == null)
            {
                return BadRequest($"Publisher with ID {bookDto.PublisherID} does not exist.");
            }

            // Validate authors exist
            var authors = await _context.Authors
                .Where(a => bookDto.AuthorIds.Contains(a.Id))
                .ToListAsync();

            if (authors.Count != bookDto.AuthorIds.Count)
            {
                return BadRequest("One or more author IDs are invalid.");
            }

            var book = new Book
            {
                Title = bookDto.Title,
                Description = bookDto.Description,
                IsRead = bookDto.IsRead,
                DateRead = bookDto.DateRead,
                Rate = bookDto.Rate,
                Genre = bookDto.Genre,
                CoverUrl = bookDto.CoverUrl,
                DateAdded = DateTime.UtcNow,
                PublisherID = bookDto.PublisherID
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Add Book-Author relationships
            foreach (var authorId in bookDto.AuthorIds)
            {
                var bookAuthor = new Book_Author
                {
                    BookId = book.Id,
                    AuthorId = authorId
                };
                _context.Books_Authors.Add(bookAuthor);
            }

            await _context.SaveChangesAsync();

            // Reload the book with related data
            var createdBook = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == book.Id);

            var responseDto = new BookResponseDto
            {
                Id = createdBook.Id,
                Title = createdBook.Title,
                Description = createdBook.Description,
                IsRead = createdBook.IsRead,
                DateRead = createdBook.DateRead,
                Rate = createdBook.Rate,
                Genre = createdBook.Genre,
                CoverUrl = createdBook.CoverUrl,
                DateAdded = createdBook.DateAdded,
                PublisherID = createdBook.PublisherID,
                PublisherName = createdBook.Publisher.Name,
                Authors = createdBook.Book_Authors.Select(ba => new AuthorResponseDto
                {
                    Id = ba.Author.Id,
                    FullName = ba.Author.FullName
                }).ToList()
            };

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, responseDto);
        }

        // PUT: api/Books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookCreateRequestDto bookDto)
        {
            var book = await _context.Books
                .Include(b => b.Book_Authors)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound($"Book with ID {id} was not found.");
            }

            // Validate publisher exists
            var publisher = await _context.Publishers.FindAsync(bookDto.PublisherID);
            if (publisher == null)
            {
                return BadRequest($"Publisher with ID {bookDto.PublisherID} does not exist.");
            }

            // Validate authors exist
            var authors = await _context.Authors
                .Where(a => bookDto.AuthorIds.Contains(a.Id))
                .ToListAsync();

            if (authors.Count != bookDto.AuthorIds.Count)
            {
                return BadRequest("One or more author IDs are invalid.");
            }

            // Update book properties
            book.Title = bookDto.Title;
            book.Description = bookDto.Description;
            book.IsRead = bookDto.IsRead;
            book.DateRead = bookDto.DateRead;
            book.Rate = bookDto.Rate;
            book.Genre = bookDto.Genre;
            book.CoverUrl = bookDto.CoverUrl;
            book.PublisherID = bookDto.PublisherID;

            // Remove existing Book-Author relationships
            _context.Books_Authors.RemoveRange(book.Book_Authors);

            // Add new Book-Author relationships
            foreach (var authorId in bookDto.AuthorIds)
            {
                var bookAuthor = new Book_Author
                {
                    BookId = book.Id,
                    AuthorId = authorId
                };
                _context.Books_Authors.Add(bookAuthor);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Book_Authors)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound($"Book with ID {id} was not found.");
            }

            // Remove Book-Author relationships first
            _context.Books_Authors.RemoveRange(book.Book_Authors);
            
            // Remove the book
            _context.Books.Remove(book);
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}