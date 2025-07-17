namespace WorkflowEngine.DTOs;

public class CreateWorkflowDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<StateDto> States { get; set; } = new();
    public List<ActionDto> Actions { get; set; } = new();
}

public class StateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public bool Enabled { get; set; } = true;
    public string? Description { get; set; }
}

public class ActionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public List<string> FromStates { get; set; } = new();
    public string ToState { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class WorkflowDefinitionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<StateDto> States { get; set; } = new();
    public List<ActionDto> Actions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
} 