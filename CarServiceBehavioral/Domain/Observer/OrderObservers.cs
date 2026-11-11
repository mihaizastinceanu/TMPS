using CarServiceBehavioral.Domain.Models;

namespace CarServiceBehavioral.Domain.Observer;

/// <summary>
/// Sends an SMS to the owner only for the changes the customer cares about.
/// The way the message is sent is given from outside (the client prints it).
/// </summary>
public class CustomerSmsNotifier : IOrderObserver
{
    private readonly Action<string> _send;

    public CustomerSmsNotifier(Action<string> send)
    {
        _send = send;
    }

    public void Update(OrderStatusChange change)
    {
        var order = change.Order;
        var text = change.To switch
        {
            OrderStatus.WaitingForParts => $"we are waiting for a part ({change.Note}), the repair will take longer.",
            OrderStatus.ReadyForPickup => $"your {order.Vehicle.Make} {order.Vehicle.Model} is ready, you can pick it up.",
            OrderStatus.Cancelled => $"order #{order.Number} was cancelled: {change.Note}.",
            _ => null
        };

        if (text is not null)
            _send($"SMS to {order.Vehicle.OwnerName}: {text}");
    }
}

/// <summary>
/// The screen in the workshop hall: logs every change and knows the current status of each order.
/// </summary>
public class WorkshopDashboard : IOrderObserver
{
    private readonly Action<string> _log;
    private readonly List<OrderStatusChange> _history = new();
    private readonly Dictionary<int, OrderStatus> _current = new();

    public WorkshopDashboard(Action<string> log)
    {
        _log = log;
    }

    public IReadOnlyList<OrderStatusChange> History => _history;

    public void Update(OrderStatusChange change)
    {
        _history.Add(change);
        _current[change.Order.Number] = change.To;

        var note = change.Note is null ? "" : $" ({change.Note})";
        _log($"[dashboard] Order #{change.Order.Number}: {change.From} -> {change.To}{note}");
    }

    public int CountIn(OrderStatus status) => _current.Values.Count(s => s == status);
}

/// <summary>
/// Accounting reacts only when a car is delivered and records the paid invoice.
/// </summary>
public class AccountingDepartment : IOrderObserver
{
    private readonly List<Invoice> _invoices = new();

    public IReadOnlyList<Invoice> Invoices => _invoices;

    public decimal Revenue => _invoices.Sum(i => i.Total);

    public void Update(OrderStatusChange change)
    {
        if (change.To == OrderStatus.Delivered && change.Order.Invoice is not null)
            _invoices.Add(change.Order.Invoice);
    }
}
