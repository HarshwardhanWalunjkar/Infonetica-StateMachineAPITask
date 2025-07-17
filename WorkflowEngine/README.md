# Configurable Workflow Engine (State-Machine API)

A minimal backend service that implements a configurable workflow engine using state machines. Built with .NET 8 and ASP.NET Core minimal APIs.

## Features

- **Workflow Definitions**: Create and manage configurable state machines with states and actions
- **Workflow Instances**: Start and execute workflow instances with full validation
- **State Transitions**: Execute actions to move between states with comprehensive validation
- **History Tracking**: Complete audit trail of all state transitions
- **RESTful API**: Clean, intuitive API endpoints for all operations

## Quick Start

### Prerequisites
- .NET 8.0 SDK or later

### Running the Application

1. **Clone and navigate to the project:**
   ```bash
   cd WorkflowEngine
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Access the API:**
   - API Base URL: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

## API Endpoints

### Workflow Definitions

#### Create Workflow Definition
```http
POST /api/workflow-definitions
Content-Type: application/json

{
  "name": "Order Processing",
  "description": "Simple order processing workflow",
  "states": [
    {
      "id": "pending",
      "name": "Pending",
      "isInitial": true,
      "isFinal": false,
      "enabled": true
    },
    {
      "id": "approved",
      "name": "Approved",
      "isInitial": false,
      "isFinal": true,
      "enabled": true
    },
    {
      "id": "rejected",
      "name": "Rejected",
      "isInitial": false,
      "isFinal": true,
      "enabled": true
    }
  ],
  "actions": [
    {
      "id": "approve",
      "name": "Approve Order",
      "enabled": true,
      "fromStates": ["pending"],
      "toState": "approved"
    },
    {
      "id": "reject",
      "name": "Reject Order",
      "enabled": true,
      "fromStates": ["pending"],
      "toState": "rejected"
    }
  ]
}
```

#### Get All Workflow Definitions
```http
GET /api/workflow-definitions
```

#### Get Workflow Definition by ID
```http
GET /api/workflow-definitions/{id}
```

### Workflow Instances

#### Create Workflow Instance
```http
POST /api/workflow-instances
Content-Type: application/json

{
  "definitionId": "workflow-definition-id"
}
```

#### Get All Workflow Instances
```http
GET /api/workflow-instances
```

#### Get Workflow Instance by ID
```http
GET /api/workflow-instances/{id}
```

#### Execute Action on Workflow Instance
```http
POST /api/workflow-instances/{instanceId}/execute
Content-Type: application/json

{
  "actionId": "approve"
}
```

## Core Concepts

### State
- **Required attributes**: `id`, `name`, `isInitial`, `isFinal`, `enabled`
- **Optional attributes**: `description`
- Each workflow definition must have exactly one initial state

### Action (Transition)
- **Required attributes**: `id`, `name`, `enabled`, `fromStates`, `toState`
- **Optional attributes**: `description`
- Actions can originate from multiple states but always end in one state

### Workflow Definition
- Collection of states and actions
- Must contain exactly one initial state
- Validated for consistency and completeness

### Workflow Instance
- Reference to its definition
- Current state tracking
- Complete history of all transitions with timestamps

## Validation Rules

### Workflow Definition Validation
- No duplicate state or action IDs
- Exactly one initial state required
- All action references must point to existing states
- Actions cannot reference non-existent states

### Action Execution Validation
- Action must belong to the instance's definition
- Action must be enabled
- Current state must be in the action's `fromStates`
- Cannot execute actions on final states

## Example Usage

### 1. Create a Simple Approval Workflow

```bash
curl -X POST http://localhost:5000/api/workflow-definitions \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Document Approval",
    "states": [
      {"id": "draft", "name": "Draft", "isInitial": true, "isFinal": false},
      {"id": "review", "name": "Under Review", "isInitial": false, "isFinal": false},
      {"id": "approved", "name": "Approved", "isInitial": false, "isFinal": true},
      {"id": "rejected", "name": "Rejected", "isInitial": false, "isFinal": true}
    ],
    "actions": [
      {"id": "submit", "name": "Submit for Review", "fromStates": ["draft"], "toState": "review"},
      {"id": "approve", "name": "Approve", "fromStates": ["review"], "toState": "approved"},
      {"id": "reject", "name": "Reject", "fromStates": ["review"], "toState": "rejected"}
    ]
  }'
```

### 2. Start a Workflow Instance

```bash
curl -X POST http://localhost:5000/api/workflow-instances \
  -H "Content-Type: application/json" \
  -d '{"definitionId": "returned-definition-id"}'
```

### 3. Execute Actions

```bash
# Submit for review
curl -X POST http://localhost:5000/api/workflow-instances/{instance-id}/execute \
  -H "Content-Type: application/json" \
  -d '{"actionId": "submit"}'

# Approve
curl -X POST http://localhost:5000/api/workflow-instances/{instance-id}/execute \
  -H "Content-Type: application/json" \
  -d '{"actionId": "approve"}'
```

## Assumptions and Design Decisions

### Storage
- **In-memory storage**: Data is stored in memory for simplicity as specified in requirements
- **No persistence**: Data is lost on application restart
- **Simple structure**: Using dictionaries for fast lookups

### API Design
- **RESTful endpoints**: Following REST conventions for clarity
- **JSON payloads**: All requests and responses use JSON format
- **HTTP status codes**: Proper status codes for different scenarios
- **Error handling**: Descriptive error messages for validation failures

### Validation
- **Comprehensive validation**: All state machine rules are enforced
- **Graceful error handling**: Invalid operations return helpful error messages
- **Pre-execution validation**: All validations happen before state changes

### Extensibility
- **Service-oriented architecture**: Clear separation of concerns
- **Interface-ready**: Easy to add interfaces for dependency injection
- **Modular design**: Services can be easily extended or replaced

## Known Limitations

1. **No persistence**: Data is lost on application restart
2. **No authentication/authorization**: All endpoints are publicly accessible
3. **No concurrent access handling**: Not designed for high-concurrency scenarios
4. **No workflow versioning**: Cannot modify existing workflow definitions
5. **No workflow templates**: Each workflow must be defined from scratch

## Future Enhancements (TODO)

- [ ] Add database persistence (Entity Framework Core)
- [ ] Implement workflow definition versioning
- [ ] Add authentication and authorization
- [ ] Support for workflow templates
- [ ] Add unit tests for all services
- [ ] Implement workflow instance archiving
- [ ] Add workflow performance metrics
- [ ] Support for conditional transitions
- [ ] Add workflow instance timeouts

## Project Structure

```
WorkflowEngine/
├── Models/                 # Domain models
│   ├── State.cs
│   ├── Action.cs
│   ├── WorkflowDefinition.cs
│   └── WorkflowInstance.cs
├── DTOs/                   # Data transfer objects
│   ├── WorkflowDefinitionDto.cs
│   └── WorkflowInstanceDto.cs
├── Services/               # Business logic services
│   ├── WorkflowService.cs
│   ├── WorkflowValidationService.cs
│   └── WorkflowStorageService.cs
├── Program.cs              # Application entry point
└── README.md              # This file
```

## Running Tests

Currently, no unit tests are included as per the requirements. The focus was on implementing the core functionality within the time constraint.

## License

This project is created as part of a take-home exercise for Infotenica. 