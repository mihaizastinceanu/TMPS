using CarServiceBehavioral.Domain.Models;

namespace CarServiceBehavioral.Domain.State;

/// <summary>
/// State: base class for the states of a ServiceOrder.
/// By default every action is forbidden; each concrete state overrides only
/// the actions that are allowed in it and decides which state comes next.
/// </summary>
public abstract class OrderState
{
    public abstract OrderStatus Status { get; }

    public virtual void StartWork(ServiceOrder order) => throw NotAllowed(order, "start the work");
    public virtual void WaitForParts(ServiceOrder order, string missingPart) => throw NotAllowed(order, "wait for parts");
    public virtual void PartsArrived(ServiceOrder order) => throw NotAllowed(order, "receive parts");
    public virtual void FinishWork(ServiceOrder order) => throw NotAllowed(order, "finish the work");
    public virtual void Deliver(ServiceOrder order, Invoice invoice) => throw NotAllowed(order, "deliver the car");
    public virtual void Cancel(ServiceOrder order, string reason) => throw NotAllowed(order, "cancel the order");

    private InvalidOperationException NotAllowed(ServiceOrder order, string action) =>
        new($"Order #{order.Number} is {Status}: cannot {action}.");
}
