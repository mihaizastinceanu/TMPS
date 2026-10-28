namespace CarServiceCreational.Domain.Models;

/// <summary>
/// A repair bay in the workshop. Bays are not created per car: they are reused through RepairBayPool.
/// </summary>
public class RepairBay
{
    public int Number { get; }
    public Vehicle? CurrentVehicle { get; private set; }
    public bool IsOccupied => CurrentVehicle is not null;

    internal RepairBay(int number)
    {
        Number = number;
    }

    internal void Occupy(Vehicle vehicle) => CurrentVehicle = vehicle;

    /// <summary>
    /// Clears the bay so the next car gets it in a clean state.
    /// </summary>
    internal void Reset() => CurrentVehicle = null;
}
