using AutoFixture;
using FinProjectTaskTracker.Data;
using FinProjectTaskTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinProjectTaskTracker.Seeders;

public static class K6DataSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        var fixture = new Fixture();

        fixture.Behaviors.Remove(
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().First());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var random = new Random();
        
        var boardId = Guid.Parse("9f08feda-35f4-422b-83e3-109b1cd0da38");

        var board = await dbContext.Boards
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board == null)
        {
            board = new Board
            {
                Id = boardId,
                Name = "Performance Test Board",
                Description = "Seeded for k6 load testing",
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.Boards.AddAsync(board);
            await dbContext.SaveChangesAsync();
        }

   
        List<Assignee> assignees;

        if (!await dbContext.Assignees.AnyAsync())
        {
            assignees = fixture.Build<Assignee>()
                .Without(a => a.Id)
                .CreateMany(50)
                .ToList();

            await dbContext.Assignees.AddRangeAsync(assignees);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            assignees = await dbContext.Assignees.ToListAsync();
        }

        var taskCount = await dbContext.Tasks.CountAsync();

        if (taskCount >= 10000)
            return;

        var statuses = Enum.GetValues<Status>();

        var tasks = new List<TaskItem>();

        for (int i = 0; i < 10000 - taskCount; i++)
        {
            tasks.Add(fixture.Build<TaskItem>() 
                .Without(t => t.Id) 
                .With(t => t.BoardId, board.Id)
                .With(t => t.AssigneeId, assignees[random.Next(assignees.Count)].Id)
                .With(t => t.Status, statuses[random.Next(statuses.Length)])
                .With(t => t.CreatedAt, DateTime.UtcNow.AddMinutes(-random.Next(0, 5000)))
                .With(t => t.DueDate, DateTime.UtcNow.AddDays(random.Next(1, 30))) // ✔ FIX
                .Create());
        }

        await dbContext.Tasks.AddRangeAsync(tasks);
        await dbContext.SaveChangesAsync();
    }
}