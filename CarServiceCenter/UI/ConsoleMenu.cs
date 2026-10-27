using System.Globalization;
using CarServiceCenter.Interfaces;
using CarServiceCenter.Models;
using CarServiceCenter.Services;

namespace CarServiceCenter.UI;

/// <summary>
/// Shows the main menu, sends user requests to the services and prints the results.
/// It contains no business rules itself.
/// </summary>
public class ConsoleMenu
{
    private const string Separator = "------------------------------------------------------------";

    private readonly InputReader _input;
    private readonly VehicleService _vehicleService;
    private readonly ServiceOrderService _orderService;
    private readonly VehicleRemovalService _removalService;
    private readonly WorkshopStatisticsService _statisticsService;
    private readonly ServiceCatalog _catalog;

    public ConsoleMenu(
        InputReader input,
        VehicleService vehicleService,
        ServiceOrderService orderService,
        VehicleRemovalService removalService,
        WorkshopStatisticsService statisticsService,
        ServiceCatalog catalog)
    {
        _input = input;
        _vehicleService = vehicleService;
        _orderService = orderService;
        _removalService = removalService;
        _statisticsService = statisticsService;
        _catalog = catalog;
    }

    /// <summary>
    /// Runs the menu loop until the user chooses Exit.
    /// </summary>
    public void Run()
    {
        Console.WriteLine("=== Car Service Center Management System ===");

        try
        {
            bool running = true;
            while (running)
            {
                PrintMenu();
                int choice = _input.ReadInt("Choose an option: ", 0, 11);
                Console.WriteLine();
                running = HandleChoice(choice);
            }
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine();
            Console.WriteLine("Input ended.");
        }

        Console.WriteLine("Goodbye!");
    }

    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Main menu");
        Console.WriteLine("   1. Register a vehicle");
        Console.WriteLine("   2. Display all registered vehicles");
        Console.WriteLine("   3. Display vehicles sorted by registration number");
        Console.WriteLine("   4. Find a vehicle by registration number");
        Console.WriteLine("   5. Delete a vehicle");
        Console.WriteLine("   6. Display available services (sample prices)");
        Console.WriteLine("   7. Create a service order");
        Console.WriteLine("   8. Display all service orders");
        Console.WriteLine("   9. View order details and cost breakdown");
        Console.WriteLine("  10. Mark an order as completed");
        Console.WriteLine("  11. Display workshop statistics");
        Console.WriteLine("   0. Exit");
    }

    /// <summary>
    /// Runs the selected action. Returns false when the user wants to exit.
    /// </summary>
    private bool HandleChoice(int choice)
    {
        switch (choice)
        {
            case 1: RegisterVehicle(); break;
            case 2: ShowVehicles(); break;
            case 3: ShowVehiclesSorted(); break;
            case 4: FindVehicle(); break;
            case 5: DeleteVehicle(); break;
            case 6: ShowServiceCatalog(); break;
            case 7: CreateOrder(); break;
            case 8: ShowOrders(); break;
            case 9: ShowOrderDetails(); break;
            case 10: CompleteOrder(); break;
            case 11: ShowStatistics(); break;
            case 0: return false;
        }

        return true;
    }

    private void RegisterVehicle()
    {
        Console.WriteLine("--- Register a vehicle ---");
        string owner = _input.ReadText("Owner name: ");
        string make = _input.ReadText("Make: ");
        string model = _input.ReadText("Model: ");
        string registration = _input.ReadText("Registration number: ");
        int year = _input.ReadInt(
            $"Manufacturing year ({VehicleService.MinimumYear}-{VehicleService.MaximumYear}): ",
            VehicleService.MinimumYear, VehicleService.MaximumYear);

        try
        {
            Vehicle vehicle = _vehicleService.RegisterVehicle(owner, make, model, registration, year);
            Console.WriteLine($"Vehicle registered successfully with ID {vehicle.Id}.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void ShowVehicles()
    {
        PrintVehicles(_vehicleService.GetAll());
    }

    private void ShowVehiclesSorted()
    {
        if (_vehicleService.GetAll().Count > 0)
        {
            Console.WriteLine("Vehicles sorted by registration number (A-Z):");
        }

        PrintVehicles(_vehicleService.GetAllSortedByRegistrationNumber());
    }

    private void FindVehicle()
    {
        if (_vehicleService.GetAll().Count == 0)
        {
            Console.WriteLine("No vehicles registered yet.");
            return;
        }

        Console.WriteLine("Letter case and spaces are ignored: 'clr794', 'CLR 794' and 'clr 794' are the same.");
        string text = _input.ReadText("Registration number (or part of it): ");

        IReadOnlyList<Vehicle> matches = _vehicleService.SearchByRegistrationNumber(text);
        if (matches.Count == 0)
        {
            Console.WriteLine($"No vehicle found with registration number containing '{text}'.");
            return;
        }

        Console.WriteLine($"Found {matches.Count} vehicle(s):");
        PrintVehicles(matches);
    }

    private void DeleteVehicle()
    {
        if (_vehicleService.GetAll().Count == 0)
        {
            Console.WriteLine("No vehicles registered yet.");
            return;
        }

        Console.WriteLine("--- Delete a vehicle ---");
        string registration = _input.ReadText("Registration number of the vehicle to delete: ");

        Vehicle? vehicle = _vehicleService.FindByRegistrationNumber(registration);
        if (vehicle is null)
        {
            Console.WriteLine($"Error: No vehicle with registration number '{registration}' was found.");
            return;
        }

        PrintVehicles(new[] { vehicle });

        if (!_removalService.CanDelete(vehicle))
        {
            Console.WriteLine($"Error: Vehicle {vehicle.RegistrationNumber} has pending order(s) and cannot be deleted. " +
                              "Complete the order(s) first.");
            return;
        }

        if (!_input.ReadYesNo("Are you sure you want to delete this vehicle? (y/n): "))
        {
            Console.WriteLine("Deletion cancelled. The vehicle was not deleted.");
            return;
        }

        try
        {
            Vehicle deleted = _removalService.DeleteByRegistrationNumber(vehicle.RegistrationNumber);
            Console.WriteLine($"Vehicle {deleted.DisplayName} was deleted. Its completed orders are kept as history.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Prints vehicles as a table, or a message if the list is empty.
    /// </summary>
    private static void PrintVehicles(IReadOnlyList<Vehicle> vehicles)
    {
        if (vehicles.Count == 0)
        {
            Console.WriteLine("No vehicles registered yet.");
            return;
        }

        Console.WriteLine($"{"ID",-4}{"Owner",-20}{"Vehicle",-22}{"Reg. number",-14}{"Year",4}");
        Console.WriteLine(Separator);
        foreach (Vehicle vehicle in vehicles)
        {
            Console.WriteLine(
                $"{vehicle.Id,-4}{vehicle.OwnerName,-20}{vehicle.Make + " " + vehicle.Model,-22}{vehicle.RegistrationNumber,-14}{vehicle.Year,4}");
        }
    }

    private void ShowServiceCatalog()
    {
        Console.WriteLine("Available services (sample prices, not real quotations):");
        Console.WriteLine($"{"No",-4}{"Service",-24}{"Price",14}{"Time",9}  Description");
        Console.WriteLine(Separator);

        for (int i = 0; i < _catalog.Services.Count; i++)
        {
            IRepairService service = _catalog.Services[i];
            Console.WriteLine(
                $"{i + 1,-4}{service.Name,-24}{FormatMoney(service.Price),14}{service.EstimatedDurationMinutes + " min",9}  {service.Description}");
        }
    }

    private void CreateOrder()
    {
        if (_vehicleService.GetAll().Count == 0)
        {
            Console.WriteLine("No vehicles registered yet. Please register a vehicle first.");
            return;
        }

        Console.WriteLine("--- Create a service order ---");
        ShowVehicles();
        Console.WriteLine();
        int vehicleId = _input.ReadInt("Vehicle ID: ", 1, int.MaxValue);

        if (_vehicleService.FindById(vehicleId) is null)
        {
            Console.WriteLine($"Error: Vehicle with ID {vehicleId} was not found.");
            return;
        }

        Console.WriteLine();
        ShowServiceCatalog();
        Console.WriteLine();
        List<int> numbers = _input.ReadNumberList(
            "Service numbers, separated by commas (e.g. 1,3): ", 1, _catalog.Services.Count);

        List<IRepairService> selected = numbers
            .Select(number => _catalog.GetByNumber(number)!)
            .ToList();

        try
        {
            ServiceOrder order = _orderService.CreateOrder(vehicleId, selected);
            Console.WriteLine($"Service order #{order.Id} created with {order.Services.Count} service(s). " +
                              $"Total: {FormatMoney(order.TotalCost)}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void ShowOrders()
    {
        IReadOnlyList<ServiceOrder> orders = _orderService.GetAll();
        if (orders.Count == 0)
        {
            Console.WriteLine("No service orders yet.");
            return;
        }

        Console.WriteLine($"{"ID",-4}{"Vehicle",-28}{"Services",9}{"Total",16}  {"Status",-10}Created");
        Console.WriteLine(Separator);
        foreach (ServiceOrder order in orders)
        {
            Console.WriteLine(
                $"{order.Id,-4}{order.Vehicle.DisplayName,-28}{order.Services.Count,9}{FormatMoney(order.TotalCost),16}  {order.Status,-10}{FormatDate(order.CreatedAt)}");
        }
    }

    private void ShowOrderDetails()
    {
        if (_orderService.GetAll().Count == 0)
        {
            Console.WriteLine("No service orders yet.");
            return;
        }

        int orderId = _input.ReadInt("Order ID: ", 1, int.MaxValue);
        ServiceOrder? order = _orderService.FindById(orderId);
        if (order is null)
        {
            Console.WriteLine($"Error: Order with ID {orderId} was not found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Service order #{order.Id}");
        Console.WriteLine($"Status:  {order.Status}");
        Console.WriteLine($"Created: {FormatDate(order.CreatedAt)}");
        Console.WriteLine($"Vehicle: #{order.Vehicle.Id} {order.Vehicle.DisplayName}, {order.Vehicle.Year}");
        Console.WriteLine($"Owner:   {order.Vehicle.OwnerName}");
        Console.WriteLine("Cost breakdown (sample prices):");
        Console.WriteLine(Separator);

        for (int i = 0; i < order.Services.Count; i++)
        {
            IServicePriced service = order.Services[i];
            Console.WriteLine($"  {i + 1}. {service.Name,-40}{FormatMoney(service.Price),15}");
        }

        Console.WriteLine(Separator);
        Console.WriteLine($"     {"TOTAL",-40}{FormatMoney(order.TotalCost),15}");
    }

    private void CompleteOrder()
    {
        if (_orderService.GetAll().Count == 0)
        {
            Console.WriteLine("No service orders yet.");
            return;
        }

        int orderId = _input.ReadInt("Order ID to mark as completed: ", 1, int.MaxValue);
        try
        {
            ServiceOrder order = _orderService.CompleteOrder(orderId);
            Console.WriteLine($"Order #{order.Id} is now {order.Status}.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void ShowStatistics()
    {
        WorkshopStatistics stats = _statisticsService.Calculate();

        Console.WriteLine("Workshop statistics");
        Console.WriteLine(Separator);
        Console.WriteLine($"Registered vehicles:            {stats.RegisteredVehicles}");
        Console.WriteLine($"Total service orders:           {stats.TotalOrders}");
        Console.WriteLine($"Pending orders:                 {stats.PendingOrders}");
        Console.WriteLine($"Completed orders:               {stats.CompletedOrders}");
        Console.WriteLine($"Total value of all orders:      {FormatMoney(stats.TotalOrdersValue)}");
        Console.WriteLine($"Value of completed orders:      {FormatMoney(stats.CompletedOrdersValue)}");
        Console.WriteLine("Note: order values are not money received. Payments are not tracked in this version.");
    }

    private static string FormatMoney(decimal amount) =>
        amount.ToString("N2", CultureInfo.InvariantCulture) + " MDL";

    private static string FormatDate(DateTime date) =>
        date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
}
