using CarServiceCenter.Interfaces;
using CarServiceCenter.Models;

namespace CarServiceCenter.Services;

/// <summary>
/// Calculates workshop statistics. It only reads data; it never changes vehicles or orders.
/// </summary>
public class WorkshopStatisticsService
{
    private readonly VehicleService _vehicleService;
    private readonly ServiceOrderService _orderService;

    public WorkshopStatisticsService(VehicleService vehicleService, ServiceOrderService orderService)
    {
        _vehicleService = vehicleService;
        _orderService = orderService;
    }

    /// <summary>
    /// Builds a statistics snapshot from the current vehicles and orders.
    /// </summary>
    public WorkshopStatistics Calculate()
    {
        IReadOnlyList<ServiceOrder> orders = _orderService.GetAll();

        return new WorkshopStatistics(
            RegisteredVehicles: _vehicleService.GetAll().Count,
            TotalOrders: orders.Count,
            PendingOrders: CountByStatus(orders, OrderStatus.Pending),
            CompletedOrders: CountByStatus(orders, OrderStatus.Completed),
            TotalOrdersValue: orders.Sum(order => order.TotalCost),
            CompletedOrdersValue: orders
                .Where(order => order.Status == OrderStatus.Completed)
                .Sum(order => order.TotalCost));
    }

    /// <summary>
    /// Counts items in a given status. It only needs the status capability, so it depends on IStatusManageable.
    /// </summary>
    public static int CountByStatus(IEnumerable<IStatusManageable> items, OrderStatus status)
    {
        return items.Count(item => item.Status == status);
    }
}
