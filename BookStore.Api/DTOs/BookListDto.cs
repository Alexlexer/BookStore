namespace BookStore.Api.DTOs;

public record BookListDto(
    int Id,
    string Title,
    int Year,
    string? Isbn,
    string IllustratorName,
    List<string> Authors,
    List<string> Genres
);