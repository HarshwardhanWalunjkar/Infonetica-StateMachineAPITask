using WorkflowEngine.Models;
using WorkflowEngine.DTOs;

namespace WorkflowEngine.Services;

public class WorkflowService
{
    private readonly WorkflowStorageService _storage;
    private readonly WorkflowValidationService _validation;

    public WorkflowService(WorkflowStorageService storage, WorkflowValidationService validation)
    {
        _storage = storage;
        _validation = validation;
    }

    public WorkflowDefinitionResponse CreateWorkflowDefinition(CreateWorkflowDefinitionRequest request)
    {
        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            States = request.States.Select(s => new State
            {
                Id = s.Id,
                Name = s.Name,
                IsInitial = s.IsInitial,
                IsFinal = s.IsFinal,
                Enabled = s.Enabled,
                Description = s.Description
            }).ToList(),
            Actions = request.Actions.Select(a => new Models.Action
            {
                Id = a.Id,
                Name = a.Name,
                Enabled = a.Enabled,
                FromStates = a.FromStates,
                ToState = a.ToState,
                Description = a.Description
            }).ToList()
        };

        var validationResult = _validation.ValidateWorkflowDefinition(definition);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid workflow definition: {string.Join("; ", validationResult.Errors)}");
        }

        _storage.SaveDefinition(definition);

        return new WorkflowDefinitionResponse
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            States = definition.States.Select(s => new StateDto
            {
                Id = s.Id,
                Name = s.Name,
                IsInitial = s.IsInitial,
                IsFinal = s.IsFinal,
                Enabled = s.Enabled,
                Description = s.Description
            }).ToList(),
            Actions = definition.Actions.Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                Enabled = a.Enabled,
                FromStates = a.FromStates,
                ToState = a.ToState,
                Description = a.Description
            }).ToList(),
            CreatedAt = definition.CreatedAt
        };
    }

    public WorkflowDefinitionResponse? GetWorkflowDefinition(string id)
    {
        var definition = _storage.GetDefinition(id);
        if (definition == null) return null;

        return new WorkflowDefinitionResponse
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            States = definition.States.Select(s => new StateDto
            {
                Id = s.Id,
                Name = s.Name,
                IsInitial = s.IsInitial,
                IsFinal = s.IsFinal,
                Enabled = s.Enabled,
                Description = s.Description
            }).ToList(),
            Actions = definition.Actions.Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                Enabled = a.Enabled,
                FromStates = a.FromStates,
                ToState = a.ToState,
                Description = a.Description
            }).ToList(),
            CreatedAt = definition.CreatedAt
        };
    }

    public List<WorkflowDefinitionResponse> GetAllWorkflowDefinitions()
    {
        return _storage.GetAllDefinitions().Select(d => new WorkflowDefinitionResponse
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            States = d.States.Select(s => new StateDto
            {
                Id = s.Id,
                Name = s.Name,
                IsInitial = s.IsInitial,
                IsFinal = s.IsFinal,
                Enabled = s.Enabled,
                Description = s.Description
            }).ToList(),
            Actions = d.Actions.Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                Enabled = a.Enabled,
                FromStates = a.FromStates,
                ToState = a.ToState,
                Description = a.Description
            }).ToList(),
            CreatedAt = d.CreatedAt
        }).ToList();
    }

    public WorkflowInstanceResponse CreateWorkflowInstance(CreateWorkflowInstanceRequest request)
    {
        var definition = _storage.GetDefinition(request.DefinitionId);
        if (definition == null)
        {
            throw new InvalidOperationException($"Workflow definition '{request.DefinitionId}' not found");
        }

        var initialState = definition.States.FirstOrDefault(s => s.IsInitial);
        if (initialState == null)
        {
            throw new InvalidOperationException("Workflow definition has no initial state");
        }

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid().ToString(),
            DefinitionId = request.DefinitionId,
            CurrentStateId = initialState.Id,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        _storage.SaveInstance(instance);

        return new WorkflowInstanceResponse
        {
            Id = instance.Id,
            DefinitionId = instance.DefinitionId,
            CurrentStateId = instance.CurrentStateId,
            CurrentStateName = initialState.Name,
            CreatedAt = instance.CreatedAt,
            LastModifiedAt = instance.LastModifiedAt,
            IsCompleted = instance.IsCompleted,
            History = instance.History.Select(h => new WorkflowHistoryDto
            {
                ActionId = h.ActionId,
                ActionName = h.ActionName,
                FromStateId = h.FromStateId,
                ToStateId = h.ToStateId,
                Timestamp = h.Timestamp
            }).ToList()
        };
    }

    public WorkflowInstanceResponse ExecuteAction(string instanceId, ExecuteActionRequest request)
    {
        var instance = _storage.GetInstance(instanceId);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow instance '{instanceId}' not found");
        }

        var definition = _storage.GetDefinition(instance.DefinitionId);
        if (definition == null)
        {
            throw new InvalidOperationException($"Workflow definition '{instance.DefinitionId}' not found");
        }

        var action = definition.Actions.FirstOrDefault(a => a.Id == request.ActionId);
        if (action == null)
        {
            throw new InvalidOperationException($"Action '{request.ActionId}' not found in workflow definition");
        }

        var validationResult = _validation.ValidateActionExecution(instance, action, definition);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid action execution: {string.Join("; ", validationResult.Errors)}");
        }

        var fromState = definition.States.First(s => s.Id == instance.CurrentStateId);
        var toState = definition.States.First(s => s.Id == action.ToState);

        // Record the transition
        var historyEntry = new WorkflowHistory
        {
            ActionId = action.Id,
            ActionName = action.Name,
            FromStateId = instance.CurrentStateId,
            ToStateId = action.ToState,
            Timestamp = DateTime.UtcNow
        };

        instance.History.Add(historyEntry);
        instance.CurrentStateId = action.ToState;
        instance.LastModifiedAt = DateTime.UtcNow;
        instance.IsCompleted = toState.IsFinal;

        _storage.SaveInstance(instance);

        return new WorkflowInstanceResponse
        {
            Id = instance.Id,
            DefinitionId = instance.DefinitionId,
            CurrentStateId = instance.CurrentStateId,
            CurrentStateName = toState.Name,
            CreatedAt = instance.CreatedAt,
            LastModifiedAt = instance.LastModifiedAt,
            IsCompleted = instance.IsCompleted,
            History = instance.History.Select(h => new WorkflowHistoryDto
            {
                ActionId = h.ActionId,
                ActionName = h.ActionName,
                FromStateId = h.FromStateId,
                ToStateId = h.ToStateId,
                Timestamp = h.Timestamp
            }).ToList()
        };
    }

    public WorkflowInstanceResponse? GetWorkflowInstance(string id)
    {
        var instance = _storage.GetInstance(id);
        if (instance == null) return null;

        var definition = _storage.GetDefinition(instance.DefinitionId);
        var currentState = definition?.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);

        return new WorkflowInstanceResponse
        {
            Id = instance.Id,
            DefinitionId = instance.DefinitionId,
            CurrentStateId = instance.CurrentStateId,
            CurrentStateName = currentState?.Name ?? "Unknown",
            CreatedAt = instance.CreatedAt,
            LastModifiedAt = instance.LastModifiedAt,
            IsCompleted = instance.IsCompleted,
            History = instance.History.Select(h => new WorkflowHistoryDto
            {
                ActionId = h.ActionId,
                ActionName = h.ActionName,
                FromStateId = h.FromStateId,
                ToStateId = h.ToStateId,
                Timestamp = h.Timestamp
            }).ToList()
        };
    }

    public List<WorkflowInstanceResponse> GetAllWorkflowInstances()
    {
        var instances = _storage.GetAllInstances();
        var definitions = _storage.GetAllDefinitions().ToDictionary(d => d.Id);

        return instances.Select(instance =>
        {
            var definition = definitions.GetValueOrDefault(instance.DefinitionId);
            var currentState = definition?.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);

            return new WorkflowInstanceResponse
            {
                Id = instance.Id,
                DefinitionId = instance.DefinitionId,
                CurrentStateId = instance.CurrentStateId,
                CurrentStateName = currentState?.Name ?? "Unknown",
                CreatedAt = instance.CreatedAt,
                LastModifiedAt = instance.LastModifiedAt,
                IsCompleted = instance.IsCompleted,
                History = instance.History.Select(h => new WorkflowHistoryDto
                {
                    ActionId = h.ActionId,
                    ActionName = h.ActionName,
                    FromStateId = h.FromStateId,
                    ToStateId = h.ToStateId,
                    Timestamp = h.Timestamp
                }).ToList()
            };
        }).ToList();
    }
} 