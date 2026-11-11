namespace CarServiceBehavioral.Domain.Models;

/// <summary>
/// A repair bay in the workshop where the work of one order is done.
/// Bays are not created per order: they are reused through RepairBayPool.
/// </summary>
public class RepairBay
{
    public int Number { get; }
    public ServiceOrder? CurrentOrder { get; private set; }
    public Vehicle? CurrentVehicle => CurrentOrder?.Vehicle;
    public bool IsOccupied => CurrentOrder is not null;

    internal RepairBay(int number)
    {
        Number = number;
    }

    internal void Occupy(ServiceOrder order) => CurrentOrder = order;

    /// <summary>
    /// Clears the bay so the next order gets it in a clean state.
    /// </summary>
    internal void Reset() => CurrentOrder = null;
}
