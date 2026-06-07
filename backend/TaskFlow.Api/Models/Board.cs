namespace TaskFlow.Api.Models;

public class Board
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    public List<BoardColumn> Columns { get; set; } = new();
}
