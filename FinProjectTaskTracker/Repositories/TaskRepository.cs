using FinProjectTaskTracker.Data;
using FinProjectTaskTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinProjectTaskTracker.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
        => await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<TaskItem>> GetOverdueAsync()
        => await _context.Tasks
            .Where(t => t.DueDate < DateTime.UtcNow && t.Status != Status.Done)
            .ToListAsync();

    public async Task AddAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}