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
[Route("api")]
public class CardsController(AppDbContext db) : ControllerBase
{
    // Verifies the column exists and belongs to a board owned by the current user.
    private async Task<BoardColumn?> OwnedColumn(Guid columnId, Guid uid) =>
        await db.Columns
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId && c.Board!.OwnerId == uid);

    private async Task<Card?> OwnedCard(Guid cardId, Guid uid) =>
        await db.Cards
            .Include(c => c.Column).ThenInclude(col => col!.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId && c.Column!.Board!.OwnerId == uid);

    [HttpPost("columns/{columnId:guid}/cards")]
    public async Task<ActionResult<CardDto>> Create(Guid columnId, CreateCardRequest req)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var column = await OwnedColumn(columnId, uid.Value);
        if (column is null) return NotFound();

        var nextOrder = await db.Cards.Where(c => c.ColumnId == columnId)
            .Select(c => (int?)c.Order).MaxAsync() ?? -1;

        var card = new Card
        {
            Title = req.Title.Trim(),
            Description = req.Description?.Trim() ?? string.Empty,
            ColumnId = columnId,
            Order = nextOrder + 1,
        };
        db.Cards.Add(card);
        await db.SaveChangesAsync();

        return Ok(ToDto(card));
    }

    [HttpPut("cards/{cardId:guid}")]
    public async Task<ActionResult<CardDto>> Update(Guid cardId, UpdateCardRequest req)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var card = await OwnedCard(cardId, uid.Value);
        if (card is null) return NotFound();

        card.Title = req.Title.Trim();
        card.Description = req.Description?.Trim() ?? string.Empty;
        await db.SaveChangesAsync();

        return Ok(ToDto(card));
    }

    [HttpPut("cards/{cardId:guid}/move")]
    public async Task<ActionResult<CardDto>> Move(Guid cardId, MoveCardRequest req)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var card = await OwnedCard(cardId, uid.Value);
        if (card is null) return NotFound();

        var target = await OwnedColumn(req.TargetColumnId, uid.Value);
        if (target is null) return BadRequest(new { message = "Target column not found or not owned." });

        card.ColumnId = target.Id;
        card.Order = req.Order;
        await db.SaveChangesAsync();

        return Ok(ToDto(card));
    }

    [HttpDelete("cards/{cardId:guid}")]
    public async Task<IActionResult> Delete(Guid cardId)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var card = await OwnedCard(cardId, uid.Value);
        if (card is null) return NotFound();

        db.Cards.Remove(card);
        await db.SaveChangesAsync();
        return NoContent();
    }

    internal static CardDto ToDto(Card c) =>
        new(c.Id, c.Title, c.Description, c.Order, c.ColumnId, c.CreatedAt);
}
