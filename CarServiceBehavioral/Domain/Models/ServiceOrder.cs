using CarServiceBehavioral.Domain.Models.Services;
using CarServiceBehavioral.Domain.Observer;
using CarServiceBehavioral.Domain.State;

namespace CarServiceBehavioral.Domain.Models;

/// <summary>
/// A work order for one vehicle. Created only through ServiceOrderBuilder.
/// State context: what the order can do depends on its current state object.
/// Observer subject: attached observers are notified every time the status changes.
/// </summary>
public class ServiceOrder
{
    public const decimal UrgentFeePercent = 20m;

    private readonly List<IOrderObserver> _observers = new();
    private OrderState _state = new ReceivedState();

    public int Number { get; }
    public Vehicle Vehicle { get; }
    public IReadOnlyList<RepairService> Services { get; }
    public IReadOnlyList<ServicePackage> Packages { get; }
    public string? Mechanic { get; }
    public string? Notes { get; }
    public bool IsUrgent { get; }

    /// <summary>The invoice paid by the customer; set when the car is delivered.</summary>
    public Invoice? Invoice { get; private set; }

    internal ServiceOrder(int number, Vehicle vehicle, List<RepairService> services,
        List<ServicePackage> packages, string? mechanic, string? notes, bool isUrgent)
    {
        Number = number;
        Vehicle = vehicle;
        Services = services;
        Packages = packages;
        Mechanic = mechanic;
        Notes = notes;
        IsUrgent = isUrgent;
    }

    /// <summary>Cost of the services and packages, without the urgent fee.</summary>
    public decimal BaseCost => Services.Sum(s => s.TotalCost) + Packages.Sum(p => p.TotalCost);

    public decimal UrgentFee => IsUrgent ? BaseCost * UrgentFeePercent / 100m : 0m;

    public decimal TotalCost => BaseCost + UrgentFee;

    // ---------- State ----------

    public OrderStatus Status => _state.Status;

    public void StartWork() => _state.StartWork(this);
    public void WaitForParts(string missingPart) => _state.WaitForParts(this, missingPart);
    public void PartsArrived() => _state.PartsArrived(this);
    public void FinishWork() => _state.FinishWork(this);
    public void Deliver(Invoice invoice) => _state.Deliver(this, invoice);
    public void Cancel(string reason) => _state.Cancel(this, reason);

    /// <summary>
    /// Called only by the state objects: switches to the next state and notifies the observers.
    /// </summary>
    internal void TransitionTo(OrderState next, string? note = null)
    {
        var from = _state.Status;
        _state = next;
        Notify(new OrderStatusChange(this, from, next.Status, note));
    }

    internal void AttachInvoice(Invoice invoice)
    {
        if (invoice.OrderNumber != Number)
            throw new ArgumentException($"Invoice is for order #{invoice.OrderNumber}, not #{Number}.", nameof(invoice));
        Invoice = invoice;
    }

    // ---------- Observer ----------

    public void Attach(IOrderObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Detach(IOrderObserver observer) => _observers.Remove(observer);

    private void Notify(OrderStatusChange change)
    {
        // a copy, so an observer may detach itself while being notified
        foreach (var observer in _observers.ToList())
            observer.Update(change);
    }
}
