using BookStore.Domain.Enums;

namespace BookStore.Api.DTOs;

public record CreateBookDto(
    string Title,
    int PublicationYear,
    string? Isbn,
    int IllustratorId,
    List<int> AuthorIds,
    List<Genre> Genres
);