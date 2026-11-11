using CarServiceBehavioral.Domain.Models;

namespace CarServiceBehavioral.Domain.State;

/// <summary>The car was registered and waits for a mechanic.</summary>
public class ReceivedState : OrderState
{
    public override OrderStatus Status => OrderStatus.Received;

    public override void StartWork(ServiceOrder order) => order.TransitionTo(new InProgressState());

    public override void Cancel(ServiceOrder order, string reason) => order.TransitionTo(new CancelledState(), reason);
}

/// <summary>A mechanic is working on the car.</summary>
public class InProgressState : OrderState
{
    public override OrderStatus Status => OrderStatus.InProgress;

    public override void WaitForParts(ServiceOrder order, string missingPart) =>
        order.TransitionTo(new WaitingForPartsState(), missingPart);

    public override void FinishWork(ServiceOrder order) => order.TransitionTo(new ReadyForPickupState());

    public override void Cancel(ServiceOrder order, string reason) => order.TransitionTo(new CancelledState(), reason);
}

/// <summary>The work is paused until a missing part arrives.</summary>
public class WaitingForPartsState : OrderState
{
    public override OrderStatus Status => OrderStatus.WaitingForParts;

    public override void PartsArrived(ServiceOrder order) => order.TransitionTo(new InProgressState());

    public override void Cancel(ServiceOrder order, string reason) => order.TransitionTo(new CancelledState(), reason);
}

/// <summary>The repair is done; the car can be taken after paying. It can no longer be cancelled.</summary>
public class ReadyForPickupState : OrderState
{
    public override OrderStatus Status => OrderStatus.ReadyForPickup;

    public override void Deliver(ServiceOrder order, Invoice invoice)
    {
        order.AttachInvoice(invoice);
        order.TransitionTo(new DeliveredState(), $"paid {invoice.Total:F2} MDL");
    }
}

/// <summary>Final state: the customer paid and took the car.</summary>
public class DeliveredState : OrderState
{
    public override OrderStatus Status => OrderStatus.Delivered;
}

/// <summary>Final state: the order was cancelled.</summary>
public class CancelledState : OrderState
{
    public override OrderStatus Status => OrderStatus.Cancelled;
}
