using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Repositories;
using Microsoft.EntityFrameworkCore;
using FinProjectTaskTracker.Tests.Database.Fixtures;
using FinProjectTaskTracker.Data;

namespace FinProjectTaskTracker.Tests.Database;

public class BoardDatabaseTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public BoardDatabaseTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<AppDbContext> CreateCleanContextAsync()
    {
        var context = _fixture.CreateContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        return context;
    }

    [Fact]
    public async Task CreateBoard_WithUniqueName_ShouldSaveSuccessfully()
    {
        await using var context = await CreateCleanContextAsync();

        var repository = new BoardRepository(context);
        var expectedName = "Development Board";

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = expectedName,
            Description = "Board for developers",
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(board);
        
        var exists = await context.Boards
            .AnyAsync(b => b.Name == expectedName);

        Assert.True(exists);
    }

    [Fact]
    public async Task CreateBoard_WithDuplicateName_ShouldThrowException()
    {
        await using var context = await CreateCleanContextAsync();

        var repository = new BoardRepository(context);
        var boardName = "Duplicate Board";

        await repository.AddAsync(new Board
        {
            Id = Guid.NewGuid(),
            Name = boardName,
            Description = "First board",
            CreatedAt = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await repository.AddAsync(new Board
            {
                Id = Guid.NewGuid(),
                Name = boardName,
                CreatedAt = DateTime.UtcNow
            });
        });
    }
}