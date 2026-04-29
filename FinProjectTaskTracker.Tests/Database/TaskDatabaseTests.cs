using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Repositories;
using Microsoft.EntityFrameworkCore;
using FinProjectTaskTracker.Tests.Database.Fixtures;
using FinProjectTaskTracker.Data;

namespace FinProjectTaskTracker.Tests.Database;

public class TaskDatabaseTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public TaskDatabaseTests(DatabaseFixture fixture)
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
    public async Task CreateTask_WithNonExistentBoardId_ShouldThrowException()
    {
        await using var context = await CreateCleanContextAsync();

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Task without board",
            BoardId = Guid.NewGuid(),
            Status = Status.Todo,
            Priority = Priority.Medium,
            DueDate = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow
        };

        context.Tasks.Add(task);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());
    }

    [Fact]
    public async Task BulkUpdateTaskStatuses_ShouldUpdateAllTasksInDatabase()
    {
        await using var context = await CreateCleanContextAsync();

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = "Development Board",
            Description = "Board for tasks",
            CreatedAt = DateTime.UtcNow
        };

        context.Boards.Add(board);

        var tasks = Enumerable.Range(1, 20)
            .Select(i => new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = $"Task {i}",
                Description = "Test task",
                BoardId = board.Id,
                Status = Status.Todo,
                Priority = Priority.Medium,
                DueDate = DateTime.UtcNow.AddDays(3),
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        context.Tasks.AddRange(tasks);

        await context.SaveChangesAsync();

        var repository = new TaskRepository(context);

        await repository.BulkUpdateStatusAsync(
            board.Id,
            Status.Todo,
            Status.InProgress);
        
        context.ChangeTracker.Clear();
        
        var updatedTasks = await context.Tasks
            .Where(t => t.BoardId == board.Id)
            .ToListAsync();

        Assert.All(updatedTasks, task =>
        {
            Assert.Equal(Status.InProgress, task.Status);
        });
    }
}