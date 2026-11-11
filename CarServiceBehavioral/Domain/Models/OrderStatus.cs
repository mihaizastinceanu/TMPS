namespace CarServiceBehavioral.Domain.Models;

/// <summary>
/// The stages an order goes through in the workshop.
/// </summary>
public enum OrderStatus
{
    Received,
    InProgress,
    WaitingForParts,
    ReadyForPickup,
    Delivered,
    Cancelled
}
