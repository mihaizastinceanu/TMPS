using CarServiceBehavioral.Domain.Builder;
using CarServiceBehavioral.Domain.Factory.PartsFactories;
using CarServiceBehavioral.Domain.Factory.ServiceCreators;
using CarServiceBehavioral.Domain.Models;
using CarServiceBehavioral.Domain.Observer;
using CarServiceBehavioral.Domain.Pool;
using CarServiceBehavioral.Domain.Prototype;
using CarServiceBehavioral.Domain.Registry;
using CarServiceBehavioral.Domain.Strategy;

namespace CarServiceBehavioral.Client;

/// <summary>
/// The only client of the system. Orders are created with the creational patterns from Lab 1,
/// then they go through the workshop using the behavioral patterns (State, Observer, Strategy).
/// </summary>
public class WorkshopClient
{
    public void Run()
    {
        var registry = WorkshopRegistry.Instance;
        Console.WriteLine($"=== {registry.WorkshopName} ===");
        Console.WriteLine();

        // ---------- Lab 1: creating the orders (Abstract Factory, Factory Method, Prototype, Builder, Singleton) ----------
        Console.WriteLine("--- New orders ---");
        IPartsFactory oem = new OemPartsFactory();
        IPartsFactory aftermarket = new AftermarketPartsFactory();
        var catalog = PackageCatalog.CreateDefault(oem);

        var logan = new Vehicle("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);
        var corolla = new Vehicle("Maria Rusu", "Toyota", "Corolla", "K XY 777", 2021);
        var focus = new Vehicle("FastTaxi SRL", "Ford", "Focus", "B CD 456", 2016);
        var octavia = new Vehicle("Victor Lungu", "Skoda", "Octavia", "O EF 789", 2020);

        var loganOrder = new ServiceOrderBuilder()
            .ForVehicle(logan)
            .AddService(new OilChangeCreator().PrepareService(aftermarket))
            .AssignMechanic("Andrei")
            .Build();

        var corollaOrder = new ServiceOrderBuilder()
            .ForVehicle(corolla)
            .AddPackage(catalog.Get("Full Check"))
            .WithNotes("Noise from the front wheels")
            .Build();

        var focusOrder = new ServiceOrderBuilder()
            .ForVehicle(focus)
            .AddService(new BrakeReplacementCreator().PrepareService(oem))
            .AddService(new EngineDiagnosticsCreator().PrepareService(oem))
            .MarkUrgent()
            .Build();

        var octaviaOrder = new ServiceOrderBuilder()
            .ForVehicle(octavia)
            .AddService(new EngineDiagnosticsCreator().PrepareService(oem))
            .Build();

        var orders = new[] { loganOrder, corollaOrder, focusOrder, octaviaOrder };
        foreach (var order in orders)
            Console.WriteLine($"Order #{order.Number} {order.Vehicle.DisplayName}: {order.Status}, {order.TotalCost:F2} MDL");
        Console.WriteLine();

        // ---------- Observer: the same observers watch every order ----------
        var sms = new CustomerSmsNotifier(message => Console.WriteLine($"    {message}"));
        var dashboard = new WorkshopDashboard(Console.WriteLine);
        var accounting = new AccountingDepartment();
        foreach (var order in orders)
        {
            order.Attach(dashboard);
            order.Attach(sms);
            order.Attach(accounting);
        }

        // ---------- State: what an order can do depends on its current state ----------
        Console.WriteLine("--- Work in the workshop (State + Observer) ---");
        var bays = new RepairBayPool();

        var loganBay = bays.Acquire(logan);
        loganOrder.StartWork();
        var corollaBay = bays.Acquire(corolla);
        corollaOrder.StartWork();
        var focusBay = bays.Acquire(focus);
        focusOrder.StartWork();

        corollaOrder.WaitForParts("OEM brake pads");
        TryAction("Deliver the Corolla now", () => new Cashier(new RegularPricing()).Checkout(corollaOrder));

        loganOrder.FinishWork();
        bays.Release(loganBay);

        octaviaOrder.Cancel("the customer changed their mind");
        TryAction("Start work on the cancelled Octavia", octaviaOrder.StartWork);

        corollaOrder.PartsArrived();
        corollaOrder.FinishWork();
        bays.Release(corollaBay);

        focusOrder.FinishWork();
        bays.Release(focusBay);
        TryAction("Cancel the finished Focus", () => focusOrder.Cancel("too late"));
        Console.WriteLine($"Free repair bays: {bays.FreeBays}/{bays.Capacity}");
        Console.WriteLine();

        // ---------- Strategy: the cashier changes the pricing strategy for each customer ----------
        Console.WriteLine("--- Checkout (Strategy) ---");
        var cashier = new Cashier(new RegularPricing());
        PrintInvoice(cashier.Checkout(loganOrder));

        cashier.SetStrategy(new LoyaltyCardPricing());
        PrintInvoice(cashier.Checkout(corollaOrder));

        cashier.SetStrategy(new FleetContractPricing());
        PrintInvoice(cashier.Checkout(focusOrder));
        Console.WriteLine();

        // ---------- What the observers collected ----------
        Console.WriteLine("--- Summary ---");
        foreach (var order in orders)
            Console.WriteLine($"Order #{order.Number} {order.Vehicle.DisplayName}: {order.Status}");
        Console.WriteLine($"Dashboard: {dashboard.History.Count} status changes, " +
                          $"{dashboard.CountIn(OrderStatus.Delivered)} delivered, {dashboard.CountIn(OrderStatus.Cancelled)} cancelled");
        Console.WriteLine($"Accounting: {accounting.Invoices.Count} invoices, revenue {accounting.Revenue:F2} MDL");
    }

    private static void TryAction(string description, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"    {description}: done");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"    {description}: refused - {ex.Message}");
        }
    }

    private static void PrintInvoice(Invoice invoice)
    {
        Console.WriteLine($"    Invoice for order #{invoice.OrderNumber}, pricing: {invoice.PricingName}");
        Console.WriteLine($"      subtotal {invoice.Subtotal:F2}, discount {invoice.Discount:F2}, total {invoice.Total:F2} MDL");
    }
}
