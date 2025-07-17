using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class WorkflowStorageService
{
    private readonly Dictionary<string, WorkflowDefinition> _definitions = new();
    private readonly Dictionary<string, WorkflowInstance> _instances = new();

    public void SaveDefinition(WorkflowDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }

    public WorkflowDefinition? GetDefinition(string id)
    {
        return _definitions.TryGetValue(id, out var definition) ? definition : null;
    }

    public List<WorkflowDefinition> GetAllDefinitions()
    {
        return _definitions.Values.ToList();
    }

    public void SaveInstance(WorkflowInstance instance)
    {
        _instances[instance.Id] = instance;
    }

    public WorkflowInstance? GetInstance(string id)
    {
        return _instances.TryGetValue(id, out var instance) ? instance : null;
    }

    public List<WorkflowInstance> GetAllInstances()
    {
        return _instances.Values.ToList();
    }
} 