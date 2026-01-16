using System.Net;
using System.Net.Http.Json;
using BookStore.Api.Data;
using BookStore.Api.DTOs;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BookStore.IntegrationTests;

public class BookApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BookApiTests(WebApplicationFactory<Program> factory)
    {
        // 1. Generate a CONSISTENT name for this specific test run.
        // We do this OUTSIDE the lambda so both the Seed logic and the API logic 
        // share the exact same database string.
        var dbName = $"IntegrationTestDb_{Guid.NewGuid()}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 2. Remove existing SQL Server configurations
                var descriptors = services.Where(d =>
                    d.ServiceType.Name.Contains("DbContextOptions") ||
                    d.ServiceType == typeof(BookDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // 3. Add In-Memory Database using the SHARED name
                services.AddDbContext<BookDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                    options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        });
    }

    [Fact]
    public async Task CreateBook_ValidData_Returns201Created()
    {
        var client = _factory.CreateClient();

        int authorId;
        int illustratorId;

        // 1. Arrange: Seed the database
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BookDbContext>();
            db.Database.EnsureCreated();

            // Clear old data
            db.Books.RemoveRange(db.Books);
            db.Authors.RemoveRange(db.Authors);
            db.Illustrators.RemoveRange(db.Illustrators);
            await db.SaveChangesAsync();

            // Create entities
            var author = new Author { FirstName = "Integration", LastName = "Tester" };
            var illustrator = new Illustrator { FirstName = "Pixel", LastName = "Artist" };

            db.Authors.Add(author);
            db.Illustrators.Add(illustrator);
            await db.SaveChangesAsync();

            authorId = author.Id;
            illustratorId = illustrator.Id;
        }

        // 2. Arrange: Prepare DTO
        var dto = new CreateBookDto(
            Title: "Integration Test Book",
            PublicationYear: 2024,
            Isbn: "9780132350884",
            IllustratorId: illustratorId,
            AuthorIds: new List<int> { authorId },
            Genres: new List<Genre> { Genre.ScienceFiction }
        );

        // 3. Act
        var response = await client.PostAsJsonAsync("/api/books", dto);

        // 4. Assert
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API Error: {response.StatusCode}. Details: {errorContent}");
        }

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }
}