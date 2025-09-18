using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Domain;
using WebApplication1.Models.DTO;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublishersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PublishersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Publishers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublisherResponseDto>>> GetPublishers()
        {
            var publishers = await _context.Publishers
                .Include(p => p.Books)
                .ToListAsync();

            var publisherDtos = publishers.Select(publisher => new PublisherResponseDto
            {
                Id = publisher.Id,
                Name = publisher.Name,
                BookCount = publisher.Books?.Count ?? 0
            }).ToList();

            return Ok(publisherDtos);
        }

        // GET: api/Publishers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PublisherResponseDto>> GetPublisher(int id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {
                return NotFound($"Publisher with ID {id} was not found.");
            }

            var publisherDto = new PublisherResponseDto
            {
                Id = publisher.Id,
                Name = publisher.Name,
                BookCount = publisher.Books?.Count ?? 0
            };

            return Ok(publisherDto);
        }

        // POST: api/Publishers
        [HttpPost]
        public async Task<ActionResult<PublisherResponseDto>> CreatePublisher(PublisherCreateRequestDto publisherDto)
        {
            // Check if publisher with same name already exists
            var existingPublisher = await _context.Publishers
                .FirstOrDefaultAsync(p => p.Name.ToLower() == publisherDto.Name.ToLower());

            if (existingPublisher != null)
            {
                return BadRequest($"Publisher with name '{publisherDto.Name}' already exists.");
            }

            var publisher = new Publisher
            {
                Name = publisherDto.Name
            };

            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            var responseDto = new PublisherResponseDto
            {
                Id = publisher.Id,
                Name = publisher.Name,
                BookCount = 0
            };

            return CreatedAtAction(nameof(GetPublisher), new { id = publisher.Id }, responseDto);
        }

        // PUT: api/Publishers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePublisher(int id, PublisherCreateRequestDto publisherDto)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound($"Publisher with ID {id} was not found.");
            }

            // Check if another publisher with same name already exists
            var existingPublisher = await _context.Publishers
                .FirstOrDefaultAsync(p => p.Name.ToLower() == publisherDto.Name.ToLower() && p.Id != id);

            if (existingPublisher != null)
            {
                return BadRequest($"Another publisher with name '{publisherDto.Name}' already exists.");
            }

            publisher.Name = publisherDto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Publishers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {
                return NotFound($"Publisher with ID {id} was not found.");
            }

            // Check if publisher has books
            if (publisher.Books != null && publisher.Books.Any())
            {
                return BadRequest($"Cannot delete publisher '{publisher.Name}' because they have associated books. Remove the books first.");
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Publishers/5/books
        [HttpGet("{id}/books")]
        public async Task<ActionResult<IEnumerable<BookResponseDto>>> GetPublisherBooks(int id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                    .ThenInclude(b => b.Book_Authors)
                        .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {
                return NotFound($"Publisher with ID {id} was not found.");
            }

            var books = publisher.Books.Select(book => new BookResponseDto
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
                PublisherName = publisher.Name,
                Authors = book.Book_Authors.Select(ba => new AuthorResponseDto
                {
                    Id = ba.Author.Id,
                    FullName = ba.Author.FullName
                }).ToList()
            }).ToList();

            return Ok(books);
        }
    }
}