using BookStore.Domain.Enums;

namespace BookStore.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int PublicationYear { get; set; }
    public string? Isbn { get; set; }

    // Foreign Keys & Navigation Properties
    public int IllustratorId { get; set; }
    public Illustrator Illustrator { get; set; } = null!;

    public ICollection<Author> Authors { get; set; } = new List<Author>();
    public List<Genre> Genres { get; set; } = new();
}