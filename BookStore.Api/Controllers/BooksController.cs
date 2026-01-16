using BookStore.Api.Data;
using BookStore.Api.DTOs;
using BookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookService _service;
    private readonly BookDbContext _context;

    public BooksController(BookService service, BookDbContext context)
    {
        _service = service;
        _context = context;
    }

    /// <summary>
    /// Creates a new book.
    /// </summary>
    /// <param name="dto">The book creation data.</param>
    /// <returns>The ID of the created book or validation errors.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
    {
        var result = await _service.CreateBookAsync(dto);

        if (!result.Success)
        {
            // Return 400 Bad Request with the list of business/validation errors
            return BadRequest(new { Errors = result.Errors });
        }

        // Return 201 Created with the ID of the new book
        return StatusCode(201, new { Id = result.BookId });
    }

    /// <summary>
    /// Retrieves a list of books with optional filtering and sorting.
    /// </summary>
    /// <param name="author">Filter books by author name (partial match).</param>
    /// <param name="sortBy">Sort by 'title', 'year', or 'id' (default).</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookListDto>>> GetAll(
        [FromQuery] string? author,
        [FromQuery] string? sortBy)
    {
        // 1. Start with a queryable (deferred execution)
        // AsNoTracking is used for read-only queries to improve performance
        var query = _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Illustrator)
            .AsNoTracking()
            .AsQueryable();

        // 2. Apply Filtering (if parameter is provided)
        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Authors.Any(a =>
                a.LastName.Contains(author) || a.FirstName.Contains(author)));
        }

        // 3. Apply Sorting
        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(b => b.Title),
            "year" => query.OrderBy(b => b.PublicationYear),
            _ => query.OrderBy(b => b.Id) // Default sort
        };

        // 4. Projection (Map Entity -> DTO)
        // This generates an efficient SQL SELECT that retrieves only needed columns
        var books = await query.Select(b => new BookListDto(
            b.Id,
            b.Title,
            b.PublicationYear,
            b.Isbn,
            $"{b.Illustrator.FirstName} {b.Illustrator.LastName}", // Concatenate Name
            b.Authors.Select(a => $"{a.FirstName} {a.LastName}").ToList(),
            b.Genres.Select(g => g.ToString()).ToList()
        )).ToListAsync();

        return Ok(books);
    }
}