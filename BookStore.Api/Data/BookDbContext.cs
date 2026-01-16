using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Illustrator> Illustrators { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Convert List<Genre> to a single string for SQL (e.g., "Action,Drama")
        modelBuilder.Entity<Book>()
            .Property(b => b.Genres)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(Enum.Parse<Genre>)
                      .ToList());

        // 2. Enforce Unique ISBN (Business Rule)
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.Isbn)
            .IsUnique()
            .HasFilter("[Isbn] IS NOT NULL");

        // 3. Seed Initial Data (So the DB isn't empty)
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "Stephen", LastName = "King" },
            new Author { Id = 2, FirstName = "Isaac", LastName = "Asimov" },
            new Author { Id = 3, FirstName = "Marguerite", LastName = "Duras" }
        );

        modelBuilder.Entity<Illustrator>().HasData(
            new Illustrator { Id = 1, FirstName = "Gustave", LastName = "Doré" },
            new Illustrator { Id = 2, FirstName = "Norman", LastName = "Rockwell" },
            new Illustrator { Id = 3, FirstName = "Beya", LastName = "Rebaï" }
        );
    }
}