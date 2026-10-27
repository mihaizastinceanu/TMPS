using CarServiceCreational.Domain.Models;

namespace CarServiceCreational.Domain.Registry;

/// <summary>
/// Singleton: the workshop has exactly one order registry, so order numbers never repeat.
/// Lazy makes the creation thread-safe.
/// </summary>
public sealed class WorkshopRegistry
{
    private static readonly Lazy<WorkshopRegistry> _instance = new(() => new WorkshopRegistry());

    private readonly List<ServiceOrder> _orders = new();
    private readonly object _lock = new();
    private int _lastOrderNumber;

    private WorkshopRegistry() { }

    public static WorkshopRegistry Instance => _instance.Value;

    public string WorkshopName => "AutoFix Service Center";

    public IReadOnlyList<ServiceOrder> Orders
    {
        get { lock (_lock) return _orders.ToList(); }
    }

    public int NextOrderNumber()
    {
        lock (_lock) return ++_lastOrderNumber;
    }

    public void Register(ServiceOrder order)
    {
        lock (_lock) _orders.Add(order);
    }
}
