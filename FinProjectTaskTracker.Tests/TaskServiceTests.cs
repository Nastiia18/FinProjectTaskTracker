using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FinProjectTaskTracker.Services;
using FinProjectTaskTracker.Repositories;
using FinProjectTaskTracker.Models;

namespace FinProjectTaskTracker.Tests;

public class TaskServiceTests
{
  // 1. ПЕРЕХІД СТАТУСІВ
    [Fact]
    public async Task ChangeStatus_TodoToInProgress_ShouldWork()
    {
        // Arrange
        var repo = new Mock<ITaskRepository>();

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Status = Status.Todo
        };

        repo.Setup(r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        repo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        var service = new TaskService(repo.Object);

        // Act
        var result = await service.ChangeStatusAsync(task.Id, Status.InProgress);

        // Assert
        Assert.Equal(Status.InProgress, result.Status);
    }

    [Fact]
    public async Task ChangeStatus_InProgressToDone_ShouldWork()
    {
        var repo = new Mock<ITaskRepository>();

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Status = Status.InProgress
        };

        repo.Setup(r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        repo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        var service = new TaskService(repo.Object);

        var result = await service.ChangeStatusAsync(task.Id, Status.Done);

        Assert.Equal(Status.Done, result.Status);
    }

    [Fact]
    public async Task ChangeStatus_TodoToDone_ShouldThrow()
    {
        var repo = new Mock<ITaskRepository>();

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Status = Status.Todo
        };

        repo.Setup(r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var service = new TaskService(repo.Object);

        Func<Task> act = async () =>
            await service.ChangeStatusAsync(task.Id, Status.Done);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task ChangeStatus_FromDone_ShouldThrow()
    {
        var repo = new Mock<ITaskRepository>();

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Status = Status.Done
        };

        repo.Setup(r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var service = new TaskService(repo.Object);

        Func<Task> act = async () =>
            await service.ChangeStatusAsync(task.Id, Status.InProgress);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }


 // 2. PRIORITY + ASSIGNEE
    
    [Fact]
    public async Task UpdateTask_CriticalWithoutAssignee_ShouldThrow()
    {
        var repo = new Mock<ITaskRepository>();

        var existing = new TaskItem
        {
            Id = Guid.NewGuid(),
            Status = Status.Todo
        };

        var updated = new TaskItem
        {
            Title = "test",
            Description = "test",
            Priority = Priority.Critical,
            AssigneeId = null,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        repo.Setup(r => r.GetByIdAsync(existing.Id))
            .ReturnsAsync(existing);

        var service = new TaskService(repo.Object);

        Func<Task> act = async () =>
            await service.UpdateTaskAsync(existing.Id, updated);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }


   // 3. OVERDUE LOGIC
    
    [Fact]
    public async Task GetOverdue_ShouldReturnOnlyOverdueTasks()
    {
        var repo = new Mock<ITaskRepository>();

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(-2),
                Status = Status.Todo
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(2),
                Status = Status.Todo
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(-1),
                Status = Status.Done
            }
        };

        repo.Setup(r => r.GetOverdueAsync())
            .ReturnsAsync(tasks.FindAll(t =>
                t.DueDate < DateTime.UtcNow && t.Status != Status.Done));

        var service = new TaskService(repo.Object);

        var result = await service.GetOverdueAsync();

        Assert.Single(result);
        Assert.Equal(Status.Todo, result[0].Status);
    }
}