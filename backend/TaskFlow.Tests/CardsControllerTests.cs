using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Controllers;
using TaskFlow.Api.Dtos;
using Xunit;

namespace TaskFlow.Tests;

public class CardsControllerTests
{
    // Creates a board for `uid` and returns (boardId, firstColumnId, secondColumnId).
    private static async Task<(Guid board, Guid col0, Guid col1)> SeedBoard(
        TaskFlow.Api.Data.AppDbContext db, Guid uid)
    {
        var created = (CreatedAtActionResult)(await new BoardsController(db).WithUser(uid)
            .Create(new CreateBoardRequest("Board"))).Result!;
        var detail = (BoardDetailDto)created.Value!;
        return (detail.Id, detail.Columns[0].Id, detail.Columns[1].Id);
    }

    [Fact]
    public async Task Create_card_in_owned_column_succeeds_and_orders()
    {
        using var db = TestHelpers.NewContext();
        var uid = Guid.NewGuid();
        var (_, col0, _) = await SeedBoard(db, uid);
        var controller = new CardsController(db).WithUser(uid);

        var first = Assert.IsType<CardDto>(Assert.IsType<OkObjectResult>(
            (await controller.Create(col0, new CreateCardRequest("First", "desc"))).Result).Value);
        var second = Assert.IsType<CardDto>(Assert.IsType<OkObjectResult>(
            (await controller.Create(col0, new CreateCardRequest("Second", null))).Result).Value);

        Assert.Equal(0, first.Order);
        Assert.Equal(1, second.Order);
        Assert.Equal(2, await db.Cards.CountAsync());
    }

    [Fact]
    public async Task Create_card_in_foreign_column_returns_NotFound()
    {
        using var db = TestHelpers.NewContext();
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var (_, col0, _) = await SeedBoard(db, owner);

        var result = await new CardsController(db).WithUser(intruder)
            .Create(col0, new CreateCardRequest("Hack", null));

        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(0, await db.Cards.CountAsync());
    }

    [Fact]
    public async Task Move_card_changes_column_and_order()
    {
        using var db = TestHelpers.NewContext();
        var uid = Guid.NewGuid();
        var (_, col0, col1) = await SeedBoard(db, uid);
        var controller = new CardsController(db).WithUser(uid);

        var card = (CardDto)((OkObjectResult)(await controller
            .Create(col0, new CreateCardRequest("Task", null))).Result!).Value!;

        var moved = (CardDto)((OkObjectResult)(await controller
            .Move(card.Id, new MoveCardRequest(col1, 5))).Result!).Value!;

        Assert.Equal(col1, moved.ColumnId);
        Assert.Equal(5, moved.Order);
    }
}
