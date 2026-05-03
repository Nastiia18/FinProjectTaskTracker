using System.Net;
using System.Net.Http.Json;
using FinProjectTaskTracker.Data;
using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;


namespace FinProjectTaskTracker.Tests.Integration.Controllers;


public class TasksControllerTests
   : IClassFixture<CustomWebApplicationFactory>
{
   private readonly HttpClient _client;
   private readonly CustomWebApplicationFactory _factory;


   public TasksControllerTests(CustomWebApplicationFactory factory)
   {
       _factory = factory;
       _client = factory.CreateClient();
   }


   [Fact]
   public async Task UpdateTask_ShouldReturnOk_AndUpdateDataInDb()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

       var task = db.Tasks.Skip(1).First();
       var taskId = task.Id;

       var updatedTask = new
       {
           Id = task.Id,
           BoardId = task.BoardId,
           Title = "Updated title",
           Description = "Updated description",
           Status = task.Status,
           Priority = Priority.High,
           AssigneeId = task.AssigneeId,
           DueDate = DateTime.UtcNow.AddDays(5),
           CreatedAt = task.CreatedAt
       };

       var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updatedTask);

       Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       using var verifyScope = _factory.Services.CreateScope();
       var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

       var dbTask = verifyDb.Tasks.First(t => t.Id == taskId);

       Assert.Equal("Updated title", dbTask.Title);
       Assert.Equal("Updated description", dbTask.Description);
       Assert.Equal(Priority.High, dbTask.Priority);
   }

   //Зміна статусу
   
   [Fact]
   public async Task ChangeStatus_ShouldReturnOk_AndUpdateStatusInDb()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

       var task = db.Tasks.FirstOrDefault(t => t.Status == Status.Todo);
       Assert.NotNull(task);

       var taskId = task!.Id;

       var response = await _client.PatchAsJsonAsync(
           $"/api/tasks/{taskId}/status", Status.InProgress);

       Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       using var scope2 = _factory.Services.CreateScope();
       var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();

       var updatedTask = db2.Tasks.First(t => t.Id == taskId);

       Assert.Equal(Status.InProgress, updatedTask.Status);
   }


   [Fact]
   public async Task ChangeStatus_ShouldReturnBadRequest_ForInvalidTransition()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

       var task = db.Tasks.FirstOrDefault(t => t.Status == Status.Todo);

       Assert.NotNull(task);

       var taskId = task!.Id;
       var originalStatus = task.Status;

       var response = await _client.PatchAsJsonAsync(
           $"/api/tasks/{taskId}/status",
           Status.Done);

       Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

       var dbTask = db.Tasks.First(t => t.Id == taskId);

       Assert.Equal(originalStatus, dbTask.Status);
   }

   // Видалення
   [Fact]
   public async Task DeleteTask_ShouldReturnNoContent_AndRemoveFromDb()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

       var task = db.Tasks.First();
       var taskId = task.Id;
       
       var response = await _client.DeleteAsync($"/api/tasks/{taskId}");

       Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

       var deletedTask = db.Tasks.FirstOrDefault(t => t.Id == taskId);

       Assert.Null(deletedTask);
   }

   //ендпоінт прострочених завдань
   [Fact]
   public async Task GetOverdueTasks_ShouldReturnOk_AndOnlyOverdueTasks()
   {
       var response = await _client.GetAsync("/api/tasks/overdue");

       Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();

       Assert.NotNull(tasks);
       
       Assert.All(tasks, t =>
       {
           Assert.True(t.DueDate < DateTime.UtcNow);
           Assert.NotEqual(Status.Done, t.Status);
       });
       
   }
}
