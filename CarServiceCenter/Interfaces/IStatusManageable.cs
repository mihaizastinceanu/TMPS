using CarServiceCenter.Models;

namespace CarServiceCenter.Interfaces;

/// <summary>
/// The status-management capability. Only things that have a lifecycle (service orders)
/// implement it; repair services do not, so they are not forced to have a status.
/// </summary>
public interface IStatusManageable
{
    OrderStatus Status { get; }
    void MarkCompleted();
}
