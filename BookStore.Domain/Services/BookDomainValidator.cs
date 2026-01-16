using BookStore.Domain.Entities;

namespace BookStore.Domain.Services;

public static class BookDomainValidator
{
    public static List<string> Validate(Book book)
    {
        var errors = new List<string>();

        if (book.PublicationYear > DateTime.Now.Year)
            errors.Add("Publication year cannot be in the future.");

        if (book.PublicationYear < 1450)
            errors.Add("Books cannot be written before 1450.");

        if (string.IsNullOrWhiteSpace(book.Title))
            errors.Add("Title is mandatory.");

        if (book.IllustratorId <= 0)
            errors.Add("Illustrator is mandatory.");

        // ISBN Rules
        if (book.PublicationYear >= 1970)
        {
            if (string.IsNullOrWhiteSpace(book.Isbn) ||
                book.Isbn.Length != 13 ||
                !long.TryParse(book.Isbn, out _))
            {
                errors.Add("For books published after 1970, ISBN must be exactly 13 digits.");
            }
        }

        return errors;
    }
}