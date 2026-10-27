using CarServiceCreational.Domain.Models;
using CarServiceCreational.Domain.Models.Services;
using CarServiceCreational.Domain.Registry;

namespace CarServiceCreational.Domain.Builder;

/// <summary>
/// Builder: an order has many optional parts (packages, mechanic, notes, urgency),
/// so it is assembled step by step instead of using a constructor with many parameters.
/// </summary>
public class ServiceOrderBuilder
{
    private Vehicle? _vehicle;
    private readonly List<RepairService> _services = new();
    private readonly List<ServicePackage> _packages = new();
    private string? _mechanic;
    private string? _notes;
    private bool _isUrgent;

    public ServiceOrderBuilder ForVehicle(Vehicle vehicle)
    {
        _vehicle = vehicle;
        return this;
    }

    public ServiceOrderBuilder AddService(RepairService service)
    {
        _services.Add(service);
        return this;
    }

    public ServiceOrderBuilder AddPackage(ServicePackage package)
    {
        _packages.Add(package);
        return this;
    }

    public ServiceOrderBuilder AssignMechanic(string mechanic)
    {
        _mechanic = mechanic;
        return this;
    }

    public ServiceOrderBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    public ServiceOrderBuilder MarkUrgent()
    {
        _isUrgent = true;
        return this;
    }

    /// <summary>
    /// Validates the data, creates the order and registers it in the workshop registry.
    /// </summary>
    public ServiceOrder Build()
    {
        if (_vehicle is null)
            throw new InvalidOperationException("An order needs a vehicle.");
        if (_services.Count == 0 && _packages.Count == 0)
            throw new InvalidOperationException("An order needs at least one service or package.");

        var registry = WorkshopRegistry.Instance;
        var order = new ServiceOrder(registry.NextOrderNumber(), _vehicle, _services.ToList(),
            _packages.ToList(), _mechanic, _notes, _isUrgent);
        registry.Register(order);
        return order;
    }
}
