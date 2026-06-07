using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Controllers;
using TaskFlow.Api.Dtos;
using Xunit;

namespace TaskFlow.Tests;

public class BoardsControllerTests
{
    [Fact]
    public async Task Create_seeds_default_columns_and_persists()
    {
        using var db = TestHelpers.NewContext();
        var uid = Guid.NewGuid();
        var controller = new BoardsController(db).WithUser(uid);

        var result = await controller.Create(new CreateBoardRequest("My Board"));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<BoardDetailDto>(created.Value);
        Assert.Equal("My Board", dto.Title);
        Assert.Equal(3, dto.Columns.Count);
        Assert.Equal(new[] { "To Do", "In Progress", "Done" }, dto.Columns.Select(c => c.Title));

        // persisted under the right owner
        var saved = await db.Boards.Include(b => b.Columns).SingleAsync();
        Assert.Equal(uid, saved.OwnerId);
        Assert.Equal(3, saved.Columns.Count);
    }

    [Fact]
    public async Task List_returns_only_callers_boards()
    {
        using var db = TestHelpers.NewContext();
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();

        await new BoardsController(db).WithUser(alice).Create(new CreateBoardRequest("Alice A"));
        await new BoardsController(db).WithUser(alice).Create(new CreateBoardRequest("Alice B"));
        await new BoardsController(db).WithUser(bob).Create(new CreateBoardRequest("Bob"));

        var result = await new BoardsController(db).WithUser(alice).List();
        var list = Assert.IsType<List<BoardDto>>(Assert.IsType<OkObjectResult>(result.Result).Value);

        Assert.Equal(2, list.Count);
        Assert.All(list, b => Assert.StartsWith("Alice", b.Title));
    }

    [Fact]
    public async Task Get_other_users_board_returns_NotFound()
    {
        using var db = TestHelpers.NewContext();
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();

        var created = (CreatedAtActionResult)(await new BoardsController(db).WithUser(owner)
            .Create(new CreateBoardRequest("Secret"))).Result!;
        var boardId = ((BoardDetailDto)created.Value!).Id;

        var result = await new BoardsController(db).WithUser(intruder).Get(boardId);
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
