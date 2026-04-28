using AutoFixture;
using FinProjectTaskTracker.Data;
using FinProjectTaskTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinProjectTaskTracker.Tests.TestData;

public static class TestDataSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Boards.AnyAsync()) return;

        var fixture = new Fixture();
        fixture.Behaviors.Remove(fixture.Behaviors.OfType<ThrowingRecursionBehavior>().First());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var random = new Random();

        var boards = fixture.Build<Board>()
            .Without(b => b.Id) 
            .With(b => b.CreatedAt, DateTime.UtcNow) // ВИПРАВЛЕНО: Явно вказуємо UTC
            .CreateMany(100)
            .ToList();

        await dbContext.Boards.AddRangeAsync(boards);
        await dbContext.SaveChangesAsync();

        var assignees = fixture.Build<Assignee>()
            .Without(a => a.Id)
            .CreateMany(200)
            .ToList();

        await dbContext.Assignees.AddRangeAsync(assignees);
        await dbContext.SaveChangesAsync();

        var tasks = new List<TaskItem>();
        var statuses = Enum.GetValues<Status>();
        var priorities = Enum.GetValues<Priority>();

        for (int i = 0; i < 10000; i++)
        {
            var randomBoardId = boards[random.Next(boards.Count)].Id;
            var randomAssigneeId = assignees[random.Next(assignees.Count)].Id;

            var task = fixture.Build<TaskItem>()
                .Without(t => t.Id)
                .With(t => t.BoardId, randomBoardId)
                .With(t => t.AssigneeId, randomAssigneeId)
                .With(t => t.Status, statuses[random.Next(statuses.Length)])
                .With(t => t.Priority, priorities[random.Next(priorities.Length)])
                .With(t => t.CreatedAt, DateTime.UtcNow.AddMinutes(-random.Next(0, 10000)))
                .With(t => t.DueDate, DateTime.SpecifyKind(
                        DateTime.UtcNow.AddDays(random.Next(1, 20)),
                        DateTimeKind.Utc)) 
                .Create();
            
            tasks.Add(task);
        }

        await dbContext.Tasks.AddRangeAsync(tasks);
        await dbContext.SaveChangesAsync();
    }
}