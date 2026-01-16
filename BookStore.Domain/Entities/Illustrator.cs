namespace BookStore.Domain.Entities;

public class Illustrator
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}