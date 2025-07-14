# PSP In-Memory Repository System

This document describes the in-memory repository system that will later be migrated to SQL Server.

## Architecture Overview

The system uses a three-tier entity hierarchy:
- **Firm**: Top-level organization (single firm per worker instance)
- **Client**: Belongs to a firm (multiple clients per firm)
- **Universe**: Belongs to a client (multiple universes per client)

## Entity Relationships

```
Firm (1) ──→ Client (N) ──→ Universe (N)
   │              │              │
   ├─ Id          ├─ Id          ├─ Id
   └─ Code        ├─ FirmId      ├─ ClientId
                  └─ Code        └─ Code
```

## Configuration

### AppHost Configuration (Orchestration)

```csharp
// Configure workers with firm context
var workers = builder.AddProject<Projects.PSP_Template_Worker>("workers")
    .WithEnvironment("PSP__FirmCode", "PSP_FIRM")
    .WithEnvironment("PSP__Messaging__WorkerCount", "6")
    .WithReplicas(3);
```

### Worker Configuration (DI Registration)

```csharp
// In your Worker Program.cs or Startup
services.AddPSPInMemoryRepositories(); // Registers all repositories
services.AddPSPMessaging(configuration); // Includes firm-aware messaging
```

## Usage Examples

### Firm Context Service

```csharp
public class MyConsumer : IConsumer<SomeCommand>
{
    private readonly IFirmContextService _firmContext;
    
    public async Task Consume(ConsumeContext<SomeCommand> context)
    {
        var currentFirm = await _firmContext.GetCurrentFirmAsync();
        // Use firm context for business logic
    }
}
```

### Repository Usage

```csharp
public class SomeService
{
    private readonly IClientRepository _clientRepo;
    private readonly IUniverseRepository _universeRepo;
    
    public async Task ProcessClientUniverse(string clientCode, string universeCode)
    {
        var client = await _clientRepo.GetByCodeAsync(clientCode);
        var universe = await _universeRepo.GetByCodeAndClientAsync(universeCode, client.Id);
        
        // Business logic using actual entities
    }
}
```

## Default Seed Data

The in-memory repositories automatically seed with:

### Firm
- Id: 1, Code: "PSP_FIRM"

### Clients (for PSP_FIRM)
- Id: 1, Code: "CLIENT1", FirmId: 1
- Id: 2, Code: "CLIENT2", FirmId: 1  
- Id: 3, Code: "CLIENT3", FirmId: 1

### Universes (for each client)
- Code: "FXUNIVERSE"
- Code: "SOIUNIVERSE" 
- Code: "CRYPTOUNIVERSE"

## Smart Queue Routing

The configuration automatically routes messages based on client hash:

```
firm1.client1.securitymapping.fxuniverse → worker1 (client*1*)
firm1.client2.securitymapping.soiuniverse → worker2 (client*2*)
firm1.client11.securitymapping.cryptouniverse → worker1 (client*1*)
```

## Benefits

✅ **Dynamic Scaling**: Handles any number of clients/universes automatically  
✅ **Zero Configuration**: Auto-discovery of entities via repositories  
✅ **Firm Isolation**: Each worker processes only its assigned firm  
✅ **Future-Ready**: Easy migration path to SQL Server  
✅ **Maximum Parallelism**: 72+ concurrent messages with smart routing

## Migration to SQL Server

When ready, simply replace:

```csharp
// From this:
services.AddPSPInMemoryRepositories();

// To this:
services.AddPSPSqlServerRepositories(connectionString);
```

The repository interfaces remain the same, ensuring zero business logic changes!
