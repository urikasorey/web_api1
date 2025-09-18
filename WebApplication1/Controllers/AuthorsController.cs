using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Domain;
using WebApplication1.Models.DTO;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Authors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorResponseDto>>> GetAuthors()
        {
            var authors = await _context.Authors
                .Include(a => a.Book_Authors)
                .ToListAsync();

            var authorDtos = authors.Select(author => new AuthorResponseDto
            {
                Id = author.Id,
                FullName = author.FullName,
                BookCount = author.Book_Authors?.Count ?? 0
            }).ToList();

            return Ok(authorDtos);
        }

        // GET: api/Authors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorResponseDto>> GetAuthor(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Book_Authors)
                    .ThenInclude(ba => ba.Book)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound($"Author with ID {id} was not found.");
            }

            var authorDto = new AuthorResponseDto
            {
                Id = author.Id,
                FullName = author.FullName,
                BookCount = author.Book_Authors?.Count ?? 0
            };

            return Ok(authorDto);
        }

        // POST: api/Authors
        [HttpPost]
        public async Task<ActionResult<AuthorResponseDto>> CreateAuthor(AuthorCreateRequestDto authorDto)
        {
            // Check if author with same name already exists
            var existingAuthor = await _context.Authors
                .FirstOrDefaultAsync(a => a.FullName.ToLower() == authorDto.FullName.ToLower());

            if (existingAuthor != null)
            {
                return BadRequest($"Author with name '{authorDto.FullName}' already exists.");
            }

            var author = new Author
            {
                FullName = authorDto.FullName
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var responseDto = new AuthorResponseDto
            {
                Id = author.Id,
                FullName = author.FullName,
                BookCount = 0
            };

            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, responseDto);
        }

        // PUT: api/Authors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorCreateRequestDto authorDto)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound($"Author with ID {id} was not found.");
            }

            // Check if another author with same name already exists
            var existingAuthor = await _context.Authors
                .FirstOrDefaultAsync(a => a.FullName.ToLower() == authorDto.FullName.ToLower() && a.Id != id);

            if (existingAuthor != null)
            {
                return BadRequest($"Another author with name '{authorDto.FullName}' already exists.");
            }

            author.FullName = authorDto.FullName;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Authors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Book_Authors)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound($"Author with ID {id} was not found.");
            }

            // Check if author has books
            if (author.Book_Authors != null && author.Book_Authors.Any())
            {
                return BadRequest($"Cannot delete author '{author.FullName}' because they have associated books. Remove the books first.");
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Authors/5/books
        [HttpGet("{id}/books")]
        public async Task<ActionResult<IEnumerable<BookResponseDto>>> GetAuthorBooks(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Book_Authors)
                    .ThenInclude(ba => ba.Book)
                        .ThenInclude(b => b.Publisher)
                .Include(a => a.Book_Authors)
                    .ThenInclude(ba => ba.Book)
                        .ThenInclude(b => b.Book_Authors)
                            .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound($"Author with ID {id} was not found.");
            }

            var books = author.Book_Authors.Select(ba => new BookResponseDto
            {
                Id = ba.Book.Id,
                Title = ba.Book.Title,
                Description = ba.Book.Description,
                IsRead = ba.Book.IsRead,
                DateRead = ba.Book.DateRead,
                Rate = ba.Book.Rate,
                Genre = ba.Book.Genre,
                CoverUrl = ba.Book.CoverUrl,
                DateAdded = ba.Book.DateAdded,
                PublisherID = ba.Book.PublisherID,
                PublisherName = ba.Book.Publisher.Name,
                Authors = ba.Book.Book_Authors.Select(bookAuthor => new AuthorResponseDto
                {
                    Id = bookAuthor.Author.Id,
                    FullName = bookAuthor.Author.FullName
                }).ToList()
            }).ToList();

            return Ok(books);
        }
    }
}