using WorkflowEngine.Services;
using WorkflowEngine.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Register workflow services
builder.Services.AddSingleton<WorkflowStorageService>();
builder.Services.AddSingleton<WorkflowValidationService>();
builder.Services.AddScoped<WorkflowService>();

var app = builder.Build();

// No Swagger UI or Swagger generation

// Root endpoint returns a plain message
app.MapGet("/", () => Results.Ok(new { message = "WorkflowEngine API is running." }));

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Workflow Definition endpoints
app.MapPost("/api/workflow-definitions", (WorkflowService service, CreateWorkflowDefinitionRequest request) =>
{
    try
    {
        var result = service.CreateWorkflowDefinition(request);
        return Results.Created($"/api/workflow-definitions/{result.Id}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/workflow-definitions", (WorkflowService service) =>
{
    var definitions = service.GetAllWorkflowDefinitions();
    return Results.Ok(definitions);
});

app.MapGet("/api/workflow-definitions/{id}", (WorkflowService service, string id) =>
{
    var definition = service.GetWorkflowDefinition(id);
    if (definition == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(definition);
});

// Workflow Instance endpoints
app.MapPost("/api/workflow-instances", (WorkflowService service, CreateWorkflowInstanceRequest request) =>
{
    try
    {
        var result = service.CreateWorkflowInstance(request);
        return Results.Created($"/api/workflow-instances/{result.Id}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/workflow-instances", (WorkflowService service) =>
{
    var instances = service.GetAllWorkflowInstances();
    return Results.Ok(instances);
});

app.MapGet("/api/workflow-instances/{id}", (WorkflowService service, string id) =>
{
    var instance = service.GetWorkflowInstance(id);
    if (instance == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(instance);
});

app.MapPost("/api/workflow-instances/{id}/execute", (WorkflowService service, string id, ExecuteActionRequest request) =>
{
    try
    {
        var result = service.ExecuteAction(id, request);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run(); 