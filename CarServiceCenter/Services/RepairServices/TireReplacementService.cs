using CarServiceCenter.Interfaces;

namespace CarServiceCenter.Services.RepairServices;

/// <summary>
/// Tire replacement for a set of four tires (labour only).
/// Sample price only, not a real workshop quotation.
/// </summary>
public class TireReplacementService : IRepairService
{
    public string Name => "Tire Replacement";
    public string Description => "Mount and balance a set of 4 tires (labour)";
    public decimal Price => 600.00m;
    public int EstimatedDurationMinutes => 60;
}
