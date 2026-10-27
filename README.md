# Creational Design Patterns


## Author: Mihail Zastinceanu

----

## Objectives:

* Get familiar with the Creational DPs;
* Choose a specific domain;
* Implement at least 3 CDPs for the specific domain;


## Domain

The domain is a **car service center** (auto repair workshop), the same domain as in the previous laboratory work. The workshop registers vehicles, offers repair services (oil change, brake pads replacement, engine diagnostics) that use spare parts of different quality tiers, sells discounted service packages and creates work orders for customers.

Main entities: `Vehicle`, `RepairService` (`OilChange`, `BrakeReplacement`, `EngineDiagnostics`), spare parts (`IEngineOil`, `IBrakePads`), `ServicePackage` and `ServiceOrder`.

Language: **C# 12 / .NET 8**, console application, no frameworks or external libraries (xUnit is used only in the separate test project).


## Project Structure

```
TMPS (branch lab-1)/
├── CarServiceCreational/
│   ├── Client/
│   │   └── WorkshopClient.cs            client code that uses the patterns
│   ├── Domain/
│   │   ├── Builder/
│   │   │   └── ServiceOrderBuilder.cs   Builder
│   │   ├── Factory/
│   │   │   ├── PartsFactories/          Abstract Factory
│   │   │   └── ServiceCreators/         Factory Method
│   │   ├── Models/
│   │   │   ├── Parts/                   products of the Abstract Factory
│   │   │   ├── Services/                products of the Factory Method
│   │   │   ├── Vehicle.cs
│   │   │   ├── ServicePackage.cs        Prototype
│   │   │   └── ServiceOrder.cs          product of the Builder
│   │   ├── Prototype/
│   │   │   ├── IPrototype.cs
│   │   │   └── PackageCatalog.cs        prototype registry
│   │   └── Registry/
│   │       └── WorkshopRegistry.cs      Singleton
│   └── Program.cs
└── CarServiceCreational.Tests/          xUnit tests
```


## Used Design Patterns: 

* Singleton
* Builder
* Prototype
* Factory Method
* Abstract Factory


## Implementation

### 1. Singleton — `WorkshopRegistry`

The workshop must have only one registry of orders, otherwise two parts of the program could give the same number to two different orders. The constructor is private and the only instance is created lazily through `Lazy<T>`, which is thread-safe. The registry gives out order numbers and stores all created orders.

```csharp
public sealed class WorkshopRegistry
{
    private static readonly Lazy<WorkshopRegistry> _instance = new(() => new WorkshopRegistry());

    private WorkshopRegistry() { }

    public static WorkshopRegistry Instance => _instance.Value;

    public int NextOrderNumber()
    {
        lock (_lock) return ++_lastOrderNumber;
    }
}
```

### 2. Abstract Factory — `IPartsFactory`

A customer can choose original (OEM) parts or cheaper aftermarket parts. All parts used for one car should come from the same tier. `IPartsFactory` creates a family of related products (engine oil and brake pads), and each concrete factory (`OemPartsFactory`, `AftermarketPartsFactory`) creates only parts of its own tier. The client code works only with the interfaces `IEngineOil` and `IBrakePads`.

```csharp
public interface IPartsFactory
{
    string TierName { get; }
    IEngineOil CreateEngineOil();
    IBrakePads CreateBrakePads();
}

public class OemPartsFactory : IPartsFactory
{
    public string TierName => "OEM";
    public IEngineOil CreateEngineOil() => new OemEngineOil();
    public IBrakePads CreateBrakePads() => new OemBrakePads();
}
```

### 3. Factory Method — `RepairServiceCreator`

`RepairServiceCreator` has the common logic for preparing a service (it validates the cost) in `PrepareService`, but it does not know which concrete service to create. Each subclass overrides the factory method `CreateService` and decides what object to create. To add a new service type, a new creator is added without changing the existing code. The creators use the Abstract Factory to get the parts.

```csharp
public abstract class RepairServiceCreator
{
    protected abstract RepairService CreateService(IPartsFactory parts);

    public RepairService PrepareService(IPartsFactory parts)
    {
        var service = CreateService(parts);
        if (service.TotalCost <= 0)
            throw new InvalidOperationException($"Service '{service.Name}' has an invalid cost.");
        return service;
    }
}

public class OilChangeCreator : RepairServiceCreator
{
    protected override RepairService CreateService(IPartsFactory parts) => new OilChange(parts.CreateEngineOil());
}
```

### 4. Prototype — `ServicePackage` and `PackageCatalog`

The workshop sells ready-made packages ("Basic Maintenance", "Full Check"). Instead of creating every package from zero, `PackageCatalog` keeps templates and returns **clones** of them. A clone can be customized (renamed, a service added) without affecting the template. `Clone()` is a deep copy: the list of services is cloned too, so the copy and the template do not share mutable data.

```csharp
public ServicePackage Clone()
{
    var copy = (ServicePackage)MemberwiseClone();
    copy._services = _services.Select(s => s.Clone()).ToList();
    return copy;
}

public ServicePackage Get(string name)
{
    if (!_templates.TryGetValue(name, out var template))
        throw new KeyNotFoundException($"Package '{name}' does not exist.");
    return template.Clone();
}
```

### 5. Builder — `ServiceOrderBuilder`

A `ServiceOrder` has one required field (the vehicle) and many optional ones: services, packages, mechanic, notes, urgency. A constructor with seven parameters would be hard to read, so the order is built step by step with a fluent builder. `Build()` checks that the order is valid, takes a number from the Singleton and registers the order. The `ServiceOrder` constructor is `internal`, so orders can be created only through the builder.

```csharp
var order = new ServiceOrderBuilder()
    .ForVehicle(corolla)
    .AddPackage(catalog.Get("Full Check"))
    .WithNotes("Noise from the front wheels")
    .MarkUrgent()
    .Build();
```


## How to Run

Requires the .NET 8 SDK. From the repository root (branch `lab-1`):

```
dotnet run --project CarServiceCreational
dotnet test
```


## Conclusions / Screenshots / Results

Output of the program:

```
=== AutoFix Service Center ===
Singleton: same instance every time = True

--- Abstract Factory + Factory Method ---
Oil Change with OEM Engine Oil 5W-30: 1020.00 MDL
Oil Change with Aftermarket Engine Oil 10W-40: 730.00 MDL

--- Prototype ---
Template:   Basic Maintenance, 2 services, 1278.00 MDL
Customized: Basic Maintenance + Brakes, 3 services, 2763.00 MDL

--- Builder ---
Order #1 - Dacia Logan (C AB 123), owner Ion Popescu
  Oil Change: 730.00 MDL
  Mechanic: Andrei
  Total: 730.00 MDL

Order #2 - Toyota Corolla (K XY 777), owner Maria Rusu
  [package] Full Check (-15%): 2609.50 MDL
  Notes: Noise from the front wheels
  Urgent: +20%
  Total: 3131.40 MDL

Orders in registry: 2
```

All 11 unit tests pass (`dotnet test`). They check that the Singleton returns the same instance, that each factory creates parts of only one tier, that the factory method creates the right service, that changing a clone does not change the template, and that the builder validates the order.

In this laboratory work I implemented five creational design patterns in a car service center application. Each pattern solves a real problem of the domain: the **Singleton** keeps one registry and unique order numbers, the **Abstract Factory** guarantees that parts of different quality tiers are not mixed, the **Factory Method** lets new service types be added without changing existing code, the **Prototype** makes it cheap to create and customize service packages from templates, and the **Builder** makes creating orders with many optional fields readable and safe. The patterns also work together: the creators use the parts factory, the package templates are built with the creators, and the builder takes its order numbers from the singleton. Object Pool was not implemented, but it would fit the domain for managing a limited number of repair bays or diagnostic machines that are reused instead of created again.
