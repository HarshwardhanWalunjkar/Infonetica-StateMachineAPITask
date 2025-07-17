using WorkflowEngine.Services;
using WorkflowEngine.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register workflow services
builder.Services.AddSingleton<WorkflowStorageService>();
builder.Services.AddSingleton<WorkflowValidationService>();
builder.Services.AddScoped<WorkflowService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Root endpoint that redirects to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

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
})
.WithName("CreateWorkflowDefinition")
.WithOpenApi();

app.MapGet("/api/workflow-definitions", (WorkflowService service) =>
{
    var definitions = service.GetAllWorkflowDefinitions();
    return Results.Ok(definitions);
})
.WithName("GetAllWorkflowDefinitions")
.WithOpenApi();

app.MapGet("/api/workflow-definitions/{id}", (WorkflowService service, string id) =>
{
    var definition = service.GetWorkflowDefinition(id);
    if (definition == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(definition);
})
.WithName("GetWorkflowDefinition")
.WithOpenApi();

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
})
.WithName("CreateWorkflowInstance")
.WithOpenApi();

app.MapGet("/api/workflow-instances", (WorkflowService service) =>
{
    var instances = service.GetAllWorkflowInstances();
    return Results.Ok(instances);
})
.WithName("GetAllWorkflowInstances")
.WithOpenApi();

app.MapGet("/api/workflow-instances/{id}", (WorkflowService service, string id) =>
{
    var instance = service.GetWorkflowInstance(id);
    if (instance == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(instance);
})
.WithName("GetWorkflowInstance")
.WithOpenApi();

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
})
.WithName("ExecuteWorkflowAction")
.WithOpenApi();

app.Run();
