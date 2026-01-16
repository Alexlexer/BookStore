using BookStore.Domain.Entities;
using BookStore.Domain.Services;
using Xunit;

namespace BookStore.UnitTests;

public class BookValidationTests
{
    [Fact]
    public void Validate_ValidBook_ReturnsEmptyErrors()
    {
        var book = new Book
        {
            Title = "Clean Code",
            PublicationYear = 2008,
            IllustratorId = 1,
            Isbn = "9780132350884"
        };
        var errors = BookDomainValidator.Validate(book);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_OldBookWithoutIsbn_IsValid()
    {
        // Books before 1970 don't require an ISBN
        var book = new Book
        {
            Title = "Ancient Manual",
            PublicationYear = 1960,
            IllustratorId = 1,
            Isbn = null
        };
        var errors = BookDomainValidator.Validate(book);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_FutureYear_ReturnsError()
    {
        var book = new Book { Title = "X", PublicationYear = DateTime.Now.Year + 1, IllustratorId = 1 };
        var errors = BookDomainValidator.Validate(book);
        Assert.Contains("Publication year cannot be in the future.", errors);
    }

    [Theory]
    [InlineData(1449)] // Gutenberg press era
    [InlineData(0)]
    public void Validate_TooOld_ReturnsError(int year)
    {
        var book = new Book { Title = "X", PublicationYear = year, IllustratorId = 1 };
        var errors = BookDomainValidator.Validate(book);
        Assert.Contains("Books cannot be written before 1450.", errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_MissingTitle_ReturnsError(string? title)
    {
        // We use string? to test null inputs explicitly
        var book = new Book { Title = title!, PublicationYear = 2000, IllustratorId = 1 };
        var errors = BookDomainValidator.Validate(book);
        Assert.Contains("Title is mandatory.", errors);
    }

    [Theory]
    [InlineData("123")]             // Too short
    [InlineData("123456789012A")]  // Contains letters
    [InlineData("12345678901234")] // Too long
    public void Validate_InvalidIsbnPost1970_ReturnsError(string badIsbn)
    {
        var book = new Book { Title = "X", PublicationYear = 1980, IllustratorId = 1, Isbn = badIsbn };
        var errors = BookDomainValidator.Validate(book);
        Assert.Contains("For books published after 1970, ISBN must be exactly 13 digits.", errors);
    }
}