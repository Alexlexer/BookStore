using BookStore.Api.Data;
using BookStore.Api.DTOs;
using BookStore.Domain.Entities;
using BookStore.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Services;

public class BookService
{
    private readonly BookDbContext _context;

    public BookService(BookDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, List<string> Errors, int? BookId)> CreateBookAsync(CreateBookDto dto)
    {
        // 1. Validation: Check if Authors exist
        var authors = await _context.Authors
            .Where(a => dto.AuthorIds.Contains(a.Id))
            .ToListAsync();

        if (authors.Count != dto.AuthorIds.Count)
            return (false, new List<string> { "One or more authors not found." }, null);

        // 2. Validation: Check if Illustrator exists
        var illustratorExists = await _context.Illustrators.AnyAsync(i => i.Id == dto.IllustratorId);
        if (!illustratorExists)
            return (false, new List<string> { "Illustrator not found." }, null);

        // 3. Map DTO to Entity
        var book = new Book
        {
            Title = dto.Title,
            PublicationYear = dto.PublicationYear,
            Isbn = dto.Isbn,
            Genres = dto.Genres,
            IllustratorId = dto.IllustratorId,
            Authors = authors
        };

        // 4. Domain Validation
        var domainErrors = BookDomainValidator.Validate(book);
        if (domainErrors.Any())
            return (false, domainErrors, null);

        // 5. Business Logic: DUPLICATE CHECK
        // "A book is a duplicate if Title + Year + Author List are identical"
        var candidates = await _context.Books
            .Where(b => b.Title == book.Title && b.PublicationYear == book.PublicationYear)
            .Include(b => b.Authors)
            .ToListAsync();

        var inputAuthorIds = dto.AuthorIds.OrderBy(id => id).ToList();

        foreach (var candidate in candidates)
        {
            var candidateAuthorIds = candidate.Authors.Select(a => a.Id).OrderBy(id => id).ToList();
            if (candidateAuthorIds.SequenceEqual(inputAuthorIds))
            {
                return (false, new List<string> { "This book already exists." }, null);
            }
        }

        // 6. Save
        try
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return (true, new List<string>(), book.Id);
        }
        catch (DbUpdateException)
        {
            return (false, new List<string> { "Database error (possible duplicate ISBN)." }, null);
        }
    }
}