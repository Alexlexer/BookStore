using System.Net;
using System.Net.Http.Json;
using BookStore.Api.Data;
using BookStore.Api.DTOs;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookStore.IntegrationTests;

public class BookApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _testDbName;

    public BookApiTests(WebApplicationFactory<Program> factory)
    {
        // Generate a unique database name to ensure test isolation
        _testDbName = $"BookStore_Test_{Guid.NewGuid()}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Replace the connection string with a Test LocalDB instance
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var testConnectionString = $"Server=(localdb)\\mssqllocaldb;Database={_testDbName};Trusted_Connection=True;MultipleActiveResultSets=true";

                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:DefaultConnection", testConnectionString }
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

            // Create the real database schema
            db.Database.EnsureCreated();

            // Clean up tables to ensure a fresh start
            db.Books.ExecuteDelete();
            db.Authors.ExecuteDelete();
            db.Illustrators.ExecuteDelete();

            // Create entities without explicit IDs (let SQL Server handle Identity)
            var author = new Author { FirstName = "Integration", LastName = "Tester" };
            var illustrator = new Illustrator { FirstName = "Pixel", LastName = "Artist" };

            db.Authors.Add(author);
            db.Illustrators.Add(illustrator);

            // Save to DB to generate IDs
            await db.SaveChangesAsync();

            // Capture the IDs assigned by SQL Server
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

        try
        {
            // 3. Act: Perform the HTTP POST
            var response = await client.PostAsJsonAsync("/api/books", dto);

            // 4. Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);
        }
        finally
        {
            // Clean up: Delete the test database after execution
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookDbContext>();
                db.Database.EnsureDeleted();
            }
        }
    }
}