using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos;

public record CreateBoardRequest([Required, MaxLength(200)] string Title);

public record BoardDto(Guid Id, string Title, DateTime CreatedAt, int ColumnCount);

public record BoardDetailDto(Guid Id, string Title, DateTime CreatedAt, List<ColumnDto> Columns);

public record CreateColumnRequest([Required, MaxLength(200)] string Title);

public record ColumnDto(Guid Id, string Title, int Order, List<CardDto> Cards);

public record CreateCardRequest(
    [Required, MaxLength(300)] string Title,
    string? Description);

public record UpdateCardRequest(
    [Required, MaxLength(300)] string Title,
    string? Description);

public record MoveCardRequest(Guid TargetColumnId, int Order);

public record CardDto(Guid Id, string Title, string Description, int Order, Guid ColumnId, DateTime CreatedAt);
