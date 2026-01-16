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
        // Генерируем уникальное имя базы
        _testDbName = $"BookStore_Test_{Guid.NewGuid()}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Подменяем строку подключения на тестовую LocalDB
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

        // Переменные для хранения ID, которые выдаст база данных
        int authorId;
        int illustratorId;

        // 1. Подготовка базы данных (Seed)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BookDbContext>();

            // Создаем реальную БД
            db.Database.EnsureCreated();

            // Чистим таблицы (на случай если база осталась от прошлого прогона)
            db.Books.ExecuteDelete();
            db.Authors.ExecuteDelete();
            db.Illustrators.ExecuteDelete();

            // Создаем сущности БЕЗ указания ID (SQL сам их присвоит)
            var author = new Author { FirstName = "Integration", LastName = "Tester" };
            var illustrator = new Illustrator { FirstName = "Pixel", LastName = "Artist" };

            db.Authors.Add(author);
            db.Illustrators.Add(illustrator);

            await db.SaveChangesAsync(); // <-- Тут SQL присваивает ID

            // Запоминаем присвоенные ID, чтобы использовать их в тесте
            authorId = author.Id;
            illustratorId = illustrator.Id;
        }

        // 2. Подготовка DTO с использованием реальных ID
        var dto = new CreateBookDto(
            Title: "Integration Test Book",
            PublicationYear: 2024,
            Isbn: "9781234567890",
            IllustratorId: illustratorId,      // Используем ID из базы
            AuthorIds: new List<int> { authorId }, // Используем ID из базы
            Genres: new List<Genre> { Genre.ScienceFiction }
        );

        try
        {
            // Act
            var response = await client.PostAsJsonAsync("/api/books", dto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);
        }
        finally
        {
            // Clean up: Удаляем тестовую базу
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookDbContext>();
                db.Database.EnsureDeleted();
            }
        }
    }
}