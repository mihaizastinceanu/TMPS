using CarServiceBehavioral.Domain.Models;

namespace CarServiceBehavioral.Domain.Observer;

/// <summary>
/// Observer: anything that wants to know when an order changes its status.
/// </summary>
public interface IOrderObserver
{
    void Update(OrderStatusChange change);
}

/// <summary>
/// The data sent to the observers: which order changed, from what status to what status and why.
/// </summary>
public record OrderStatusChange(ServiceOrder Order, OrderStatus From, OrderStatus To, string? Note);
