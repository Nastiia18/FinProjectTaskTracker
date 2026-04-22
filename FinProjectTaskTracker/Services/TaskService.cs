using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Repositories;

namespace FinProjectTaskTracker.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepo;

    public TaskService(ITaskRepository taskRepo)
    {
        _taskRepo = taskRepo;
    }

    public async Task<TaskItem> UpdateTaskAsync(Guid id, TaskItem updatedTask)
    {
        var task = await _taskRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Task not found");

        if (updatedTask.DueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be in the future");

        if (updatedTask.Priority == Priority.Critical && updatedTask.AssigneeId == null)
            throw new ArgumentException("Critical tasks must have assignee");

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.Priority = updatedTask.Priority;
        task.AssigneeId = updatedTask.AssigneeId;
        task.DueDate = updatedTask.DueDate;

        await _taskRepo.UpdateAsync(task);
        return task;
    }

    public async Task<TaskItem> ChangeStatusAsync(Guid id, Status newStatus)
    {
        var task = await _taskRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Task not found");

        if (task.Status == Status.Todo && newStatus != Status.InProgress)
            throw new ArgumentException("Invalid transition");

        if (task.Status == Status.InProgress && newStatus != Status.Done)
            throw new ArgumentException("Invalid transition");

        if (task.Status == Status.Done)
            throw new ArgumentException("Cannot change status from Done");

        task.Status = newStatus;

        await _taskRepo.UpdateAsync(task);
        return task;
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var task = await _taskRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Task not found");

        await _taskRepo.DeleteAsync(task);
    }

    public async Task<List<TaskItem>> GetOverdueAsync()
        => await _taskRepo.GetOverdueAsync();
}