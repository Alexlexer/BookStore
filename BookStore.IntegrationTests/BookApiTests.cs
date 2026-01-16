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
using Microsoft.Extensions.DependencyInjection.Extensions; // Required for RemoveAll
using Xunit;

namespace BookStore.IntegrationTests;

public class BookApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BookApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 1. THE NUCLEAR OPTION
                // We must remove ALL database-related services to prevent 
                // "Mixed Database Provider" errors and ensure we don't use SQL Server on Linux.
                services.RemoveAll(typeof(DbContextOptions));
                services.RemoveAll(typeof(DbContextOptions<BookDbContext>));
                services.RemoveAll(typeof(BookDbContext));

                // 2. Add In-Memory Database (Cross-Platform)
                services.AddDbContext<BookDbContext>(options =>
                {
                    // Use a unique name to ensure isolation
                    options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}");

                    // Ignore transaction warnings (InMemory doesn't support transactions like SQL)
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

            // Ensure DB is created
            db.Database.EnsureCreated();

            // Create entities
            // InMemory handles IDs automatically, just like SQL Server
            var author = new Author { FirstName = "Integration", LastName = "Tester" };
            var illustrator = new Illustrator { FirstName = "Pixel", LastName = "Artist" };

            db.Authors.Add(author);
            db.Illustrators.Add(illustrator);

            await db.SaveChangesAsync();

            // Capture the generated IDs
            authorId = author.Id;
            illustratorId = illustrator.Id;
        }

        // 2. Arrange: Prepare DTO using the real IDs
        var dto = new CreateBookDto(
            Title: "Integration Test Book",
            PublicationYear: 2024,
            Isbn: "9781234567890",
            IllustratorId: illustratorId,
            AuthorIds: new List<int> { authorId },
            Genres: new List<Genre> { Genre.ScienceFiction }
        );

        // 3. Act: Perform the HTTP POST
        var response = await client.PostAsJsonAsync("/api/books", dto);

        // 4. Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }
}