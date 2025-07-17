using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class WorkflowValidationService
{
    public ValidationResult ValidateWorkflowDefinition(WorkflowDefinition definition)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate basic properties
        if (string.IsNullOrWhiteSpace(definition.Name))
        {
            result.Errors.Add("Workflow name is required");
            result.IsValid = false;
        }

        // Validate states
        if (!definition.States.Any())
        {
            result.Errors.Add("At least one state is required");
            result.IsValid = false;
        }

        var initialStates = definition.States.Where(s => s.IsInitial).ToList();
        if (initialStates.Count == 0)
        {
            result.Errors.Add("At least one initial state is required");
            result.IsValid = false;
        }
        else if (initialStates.Count > 1)
        {
            result.Errors.Add("Only one initial state is allowed");
            result.IsValid = false;
        }

        // Validate actions
        foreach (var action in definition.Actions)
        {
            if (string.IsNullOrWhiteSpace(action.Name))
            {
                result.Errors.Add($"Action name is required for action {action.Id}");
                result.IsValid = false;
            }

            if (string.IsNullOrWhiteSpace(action.ToState))
            {
                result.Errors.Add($"Target state is required for action {action.Id}");
                result.IsValid = false;
            }

            // Validate that ToState exists
            if (!definition.States.Any(s => s.Id == action.ToState))
            {
                result.Errors.Add($"Target state '{action.ToState}' for action '{action.Id}' does not exist");
                result.IsValid = false;
            }

            // Validate that FromStates exist
            foreach (var fromState in action.FromStates)
            {
                if (!definition.States.Any(s => s.Id == fromState))
                {
                    result.Errors.Add($"Source state '{fromState}' for action '{action.Id}' does not exist");
                    result.IsValid = false;
                }
            }
        }

        return result;
    }

    public ValidationResult ValidateActionExecution(WorkflowInstance instance, Models.Action action, WorkflowDefinition definition)
    {
        var result = new ValidationResult { IsValid = true };

        // Check if action is enabled
        if (!action.Enabled)
        {
            result.Errors.Add($"Action '{action.Id}' is disabled");
            result.IsValid = false;
        }

        // Check if current state allows this action
        if (!action.FromStates.Contains(instance.CurrentStateId))
        {
            result.Errors.Add($"Action '{action.Id}' cannot be executed from current state '{instance.CurrentStateId}'");
            result.IsValid = false;
        }

        // Check if workflow is already completed
        if (instance.IsCompleted)
        {
            result.Errors.Add("Cannot execute actions on a completed workflow instance");
            result.IsValid = false;
        }

        return result;
    }
} 