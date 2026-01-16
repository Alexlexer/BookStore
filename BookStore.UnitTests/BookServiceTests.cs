using BookStore.Api.Data;
using BookStore.Api.DTOs;
using BookStore.Api.Services;
using BookStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookStore.UnitTests;

public class BookServiceTests
{
    // Helper to create a fake database for every test run
    private BookDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BookDbContext(options);
    }

    [Fact]
    public async Task CreateBook_DuplicateTitleYearAuthors_ReturnsError()
    {
        // Arrange
        using var context = GetInMemoryContext();

        // Seed existing data
        var author = new Author { Id = 1, FirstName = "John", LastName = "Doe" };
        var illustrator = new Illustrator { Id = 10, FirstName = "Jane", LastName = "Art" };

        context.Authors.Add(author);
        context.Illustrators.Add(illustrator);
        context.Books.Add(new Book
        {
            Title = "My Book",
            PublicationYear = 2020,
            IllustratorId = 10,
            Authors = new List<Author> { author }
        });
        await context.SaveChangesAsync();

        var service = new BookService(context);

        // Try to create the EXACT same book
        var dto = new CreateBookDto(
            Title: "My Book",
            PublicationYear: 2020,
            Isbn: "9781234567890",
            IllustratorId: 10,
            AuthorIds: new List<int> { 1 },
            Genres: new()
        );

        // Act
        var result = await service.CreateBookAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("This book already exists.", result.Errors);
    }
}