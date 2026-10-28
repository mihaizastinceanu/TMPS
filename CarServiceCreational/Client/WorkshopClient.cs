using CarServiceCreational.Domain.Builder;
using CarServiceCreational.Domain.Factory.PartsFactories;
using CarServiceCreational.Domain.Factory.ServiceCreators;
using CarServiceCreational.Domain.Models;
using CarServiceCreational.Domain.Pool;
using CarServiceCreational.Domain.Prototype;
using CarServiceCreational.Domain.Registry;

namespace CarServiceCreational.Client;

/// <summary>
/// Client code: uses the creational patterns to serve the customers and prints the results.
/// </summary>
public class WorkshopClient
{
    public void Run()
    {
        // Singleton
        var registry = WorkshopRegistry.Instance;
        Console.WriteLine($"=== {registry.WorkshopName} ===");
        Console.WriteLine($"Singleton: same instance every time = {ReferenceEquals(registry, WorkshopRegistry.Instance)}");
        Console.WriteLine();

        // Abstract Factory: the customer chooses the parts tier once
        IPartsFactory oem = new OemPartsFactory();
        IPartsFactory aftermarket = new AftermarketPartsFactory();

        // Factory Method: each creator builds its own type of service
        Console.WriteLine("--- Abstract Factory + Factory Method ---");
        var oilCreator = new OilChangeCreator();
        var oemOil = oilCreator.PrepareService(oem);
        var cheapOil = oilCreator.PrepareService(aftermarket);
        Console.WriteLine($"{oemOil.Name} with {oemOil.Parts[0].Name}: {oemOil.TotalCost:F2} MDL");
        Console.WriteLine($"{cheapOil.Name} with {cheapOil.Parts[0].Name}: {cheapOil.TotalCost:F2} MDL");
        Console.WriteLine();

        // Prototype: packages are copied from templates and then customized
        Console.WriteLine("--- Prototype ---");
        var catalog = PackageCatalog.CreateDefault(oem);
        var custom = catalog.Get("Basic Maintenance");
        custom.Name = "Basic Maintenance + Brakes";
        custom.AddService(new BrakeReplacementCreator().PrepareService(oem));
        var template = catalog.Get("Basic Maintenance");
        Console.WriteLine($"Template:   {template.Name}, {template.Services.Count} services, {template.TotalCost:F2} MDL");
        Console.WriteLine($"Customized: {custom.Name}, {custom.Services.Count} services, {custom.TotalCost:F2} MDL");
        Console.WriteLine();

        // Builder: orders are assembled step by step
        Console.WriteLine("--- Builder ---");
        var logan = new Vehicle("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);
        var corolla = new Vehicle("Maria Rusu", "Toyota", "Corolla", "K XY 777", 2021);

        var firstOrder = new ServiceOrderBuilder()
            .ForVehicle(logan)
            .AddService(cheapOil)
            .AssignMechanic("Andrei")
            .Build();

        var secondOrder = new ServiceOrderBuilder()
            .ForVehicle(corolla)
            .AddPackage(catalog.Get("Full Check"))
            .WithNotes("Noise from the front wheels")
            .MarkUrgent()
            .Build();

        PrintOrder(firstOrder);
        PrintOrder(secondOrder);

        Console.WriteLine($"Orders in registry: {registry.Orders.Count}");
        Console.WriteLine();

        // Object Pool: only 3 repair bays, the 4th car waits until one is free
        Console.WriteLine("--- Object Pool ---");
        var bays = new RepairBayPool();
        var focus = new Vehicle("Elena Ciobanu", "Ford", "Focus", "B CD 456", 2016);
        var octavia = new Vehicle("Victor Lungu", "Skoda", "Octavia", "O EF 789", 2020);
        Console.WriteLine($"Repair bays: {bays.Capacity}, free: {bays.FreeBays}");

        var loganBay = bays.Acquire(logan);
        var corollaBay = bays.Acquire(corolla);
        var focusBay = bays.Acquire(focus);
        foreach (var bay in new[] { loganBay, corollaBay, focusBay })
            Console.WriteLine($"{bay.CurrentVehicle!.DisplayName} -> bay {bay.Number}");
        Console.WriteLine($"Free bays: {bays.FreeBays}");

        var waiting = bays.AcquireAsync(octavia);
        Console.WriteLine($"{octavia.DisplayName} is waiting for a free bay (got one: {waiting.IsCompleted})");

        Console.WriteLine($"Bay {loganBay.Number} finished {logan.DisplayName} and is released");
        bays.Release(loganBay);
        var octaviaBay = waiting.GetAwaiter().GetResult();
        Console.WriteLine($"{octavia.DisplayName} -> bay {octaviaBay.Number}");

        bays.Release(corollaBay);
        bays.Release(focusBay);
        bays.Release(octaviaBay);
        Console.WriteLine($"All cars are done, free bays: {bays.FreeBays}");
    }

    private static void PrintOrder(ServiceOrder order)
    {
        Console.WriteLine($"Order #{order.Number} - {order.Vehicle.DisplayName}, owner {order.Vehicle.OwnerName}");
        foreach (var package in order.Packages)
            Console.WriteLine($"  [package] {package.Name} (-{package.DiscountPercent}%): {package.TotalCost:F2} MDL");
        foreach (var service in order.Services)
            Console.WriteLine($"  {service.Name}: {service.TotalCost:F2} MDL");
        if (order.Mechanic is not null)
            Console.WriteLine($"  Mechanic: {order.Mechanic}");
        if (order.Notes is not null)
            Console.WriteLine($"  Notes: {order.Notes}");
        if (order.IsUrgent)
            Console.WriteLine($"  Urgent: +{ServiceOrder.UrgentFeePercent}%");
        Console.WriteLine($"  Total: {order.TotalCost:F2} MDL");
        Console.WriteLine();
    }
}
