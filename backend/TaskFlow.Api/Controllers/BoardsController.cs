using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BoardsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BoardDto>>> List()
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var boards = await db.Boards
            .Where(b => b.OwnerId == uid)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BoardDto(b.Id, b.Title, b.CreatedAt, b.Columns.Count))
            .ToListAsync();

        return Ok(boards);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BoardDetailDto>> Get(Guid id)
    {
        var uid = User.GetUserId();
        var board = await db.Boards
            .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Cards.OrderBy(card => card.Order))
            .FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == uid);

        if (board is null) return NotFound();

        return Ok(ToDetail(board));
    }

    [HttpPost]
    public async Task<ActionResult<BoardDetailDto>> Create(CreateBoardRequest req)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var board = new Board
        {
            Title = req.Title.Trim(),
            OwnerId = uid.Value,
            Columns =
            {
                new BoardColumn { Title = "To Do", Order = 0 },
                new BoardColumn { Title = "In Progress", Order = 1 },
                new BoardColumn { Title = "Done", Order = 2 },
            },
        };
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = board.Id }, ToDetail(board));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = User.GetUserId();
        var board = await db.Boards.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == uid);
        if (board is null) return NotFound();

        db.Boards.Remove(board);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/columns")]
    public async Task<ActionResult<ColumnDto>> AddColumn(Guid id, CreateColumnRequest req)
    {
        var uid = User.GetUserId();
        var board = await db.Boards.Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == uid);
        if (board is null) return NotFound();

        var column = new BoardColumn
        {
            Title = req.Title.Trim(),
            BoardId = board.Id,
            Order = board.Columns.Count == 0 ? 0 : board.Columns.Max(c => c.Order) + 1,
        };
        db.Columns.Add(column);
        await db.SaveChangesAsync();

        return Ok(new ColumnDto(column.Id, column.Title, column.Order, new List<CardDto>()));
    }

    internal static BoardDetailDto ToDetail(Board board) => new(
        board.Id,
        board.Title,
        board.CreatedAt,
        board.Columns
            .OrderBy(c => c.Order)
            .Select(c => new ColumnDto(
                c.Id, c.Title, c.Order,
                c.Cards.OrderBy(card => card.Order)
                    .Select(card => new CardDto(card.Id, card.Title, card.Description, card.Order, card.ColumnId, card.CreatedAt))
                    .ToList()))
            .ToList());
}
