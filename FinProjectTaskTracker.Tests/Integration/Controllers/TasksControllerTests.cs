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
   public async Task UpdateTask_ShouldReturnOk()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


       var task = db.Tasks.Skip(1).First();


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


       var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", updatedTask);


       Assert.Equal(HttpStatusCode.OK, response.StatusCode);
   }


   [Fact]
   public async Task ChangeStatus_ShouldReturnOk()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


       var task = db.Tasks.FirstOrDefault(t => t.Status == Status.Todo);
      
       Assert.NotNull(task);


       var response = await _client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", Status.InProgress);


       Assert.Equal(HttpStatusCode.OK, response.StatusCode);
   }


   [Fact]
   public async Task ChangeStatus_InvalidTransition_ShouldReturnBadRequest()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


       var task = db.Tasks.FirstOrDefault(t => t.Status == Status.Todo);
       Assert.NotNull(task);


       var response = await _client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", Status.Done);


       Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
   }


   [Fact]
   public async Task DeleteTask_ShouldReturnNoContent()
   {
       using var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      
       var task = db.Tasks.First();


       var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");


       Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
   }


   [Fact]
   public async Task GetOverdueTasks_ShouldReturnOk()
   {
       var response = await _client.GetAsync("/api/tasks/overdue");


       Assert.Equal(HttpStatusCode.OK, response.StatusCode);
   }
}
