public enum Status
{
    Todo,
    InProgress,
    Done
}


public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}


public class TaskItem
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }


    public Status Status { get; set; }
    public Priority Priority { get; set; }


    public Guid? AssigneeId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}