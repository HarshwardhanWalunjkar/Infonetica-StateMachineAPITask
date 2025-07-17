namespace WorkflowEngine.DTOs;

public class CreateWorkflowInstanceRequest
{
    public string DefinitionId { get; set; } = string.Empty;
}

public class ExecuteActionRequest
{
    public string ActionId { get; set; } = string.Empty;
}

public class WorkflowInstanceResponse
{
    public string Id { get; set; } = string.Empty;
    public string DefinitionId { get; set; } = string.Empty;
    public string CurrentStateId { get; set; } = string.Empty;
    public string CurrentStateName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public bool IsCompleted { get; set; }
    public List<WorkflowHistoryDto> History { get; set; } = new();
}

public class WorkflowHistoryDto
{
    public string ActionId { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string FromStateId { get; set; } = string.Empty;
    public string ToStateId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
} 