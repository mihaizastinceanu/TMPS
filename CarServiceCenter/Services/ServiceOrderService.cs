using CarServiceCenter.Interfaces;
using CarServiceCenter.Models;

namespace CarServiceCenter.Services;

/// <summary>
/// Handles service order operations: creating, finding, listing and completing orders.
/// Orders are stored in memory.
/// </summary>
public class ServiceOrderService
{
    private readonly VehicleService _vehicleService;
    private readonly List<ServiceOrder> _orders = new();
    private int _nextId = 1;

    public ServiceOrderService(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public IReadOnlyList<ServiceOrder> GetAll() => _orders;

    public ServiceOrder? FindById(int id) => _orders.FirstOrDefault(order => order.Id == id);

    /// <summary>
    /// Counts the orders of a vehicle that are still Pending.
    /// </summary>
    public int CountPendingOrdersForVehicle(int vehicleId)
    {
        return _orders.Count(order => order.Vehicle.Id == vehicleId && order.Status == OrderStatus.Pending);
    }

    /// <summary>
    /// Creates a Pending order for an existing vehicle. Works with any IServicePriced,
    /// so new service types need no changes here.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the vehicle does not exist or no services are given.</exception>
    public ServiceOrder CreateOrder(int vehicleId, IEnumerable<IServicePriced> services)
    {
        Vehicle vehicle = _vehicleService.FindById(vehicleId)
            ?? throw new ArgumentException($"Vehicle with ID {vehicleId} was not found.");

        List<IServicePriced> selectedServices = services.ToList();
        if (selectedServices.Count == 0)
        {
            throw new ArgumentException("A service order must contain at least one service.");
        }

        var order = new ServiceOrder(_nextId++, vehicle, selectedServices, DateTime.Now);
        _orders.Add(order);
        return order;
    }

    /// <summary>
    /// Marks the order with the given ID as Completed.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the order does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the order is already completed.</exception>
    public ServiceOrder CompleteOrder(int orderId)
    {
        ServiceOrder order = FindById(orderId)
            ?? throw new ArgumentException($"Order with ID {orderId} was not found.");

        order.MarkCompleted();
        return order;
    }
}
