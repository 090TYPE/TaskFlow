namespace TaskFlow.Api.Models;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid ColumnId { get; set; }
    public BoardColumn? Column { get; set; }
}
