using CarServiceCenter.Models;

namespace CarServiceCenter.Services;

/// <summary>
/// Removes vehicles from the register. A vehicle can be removed only if it has no Pending orders;
/// its Completed orders are kept as service history. This rule needs both vehicles and orders,
/// so it lives in its own class instead of in VehicleService or the UI.
/// </summary>
public class VehicleRemovalService
{
    private readonly VehicleService _vehicleService;
    private readonly ServiceOrderService _orderService;

    public VehicleRemovalService(VehicleService vehicleService, ServiceOrderService orderService)
    {
        _vehicleService = vehicleService;
        _orderService = orderService;
    }

    /// <summary>
    /// Returns true if the vehicle has no Pending orders and may be deleted.
    /// </summary>
    public bool CanDelete(Vehicle vehicle)
    {
        return _orderService.CountPendingOrdersForVehicle(vehicle.Id) == 0;
    }

    /// <summary>
    /// Deletes the vehicle with this registration number (case and spaces ignored).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when no vehicle has this registration number.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the vehicle still has Pending orders.</exception>
    public Vehicle DeleteByRegistrationNumber(string registrationNumber)
    {
        Vehicle vehicle = _vehicleService.FindByRegistrationNumber(registrationNumber)
            ?? throw new ArgumentException(
                $"No vehicle with registration number '{registrationNumber.Trim()}' was found.");

        int pendingOrders = _orderService.CountPendingOrdersForVehicle(vehicle.Id);
        if (pendingOrders > 0)
        {
            throw new InvalidOperationException(
                $"Vehicle {vehicle.RegistrationNumber} has {pendingOrders} pending order(s) and cannot be deleted. " +
                "Complete the order(s) first.");
        }

        _vehicleService.RemoveVehicle(vehicle.Id);
        return vehicle;
    }
}
