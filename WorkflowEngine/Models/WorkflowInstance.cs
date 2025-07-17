namespace WorkflowEngine.Models;

public class WorkflowInstance
{
    public string Id { get; set; } = string.Empty;
    public string DefinitionId { get; set; } = string.Empty;
    public string CurrentStateId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    public List<WorkflowHistory> History { get; set; } = new();
    public bool IsCompleted { get; set; }
}

public class WorkflowHistory
{
    public string ActionId { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string FromStateId { get; set; } = string.Empty;
    public string ToStateId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
} 