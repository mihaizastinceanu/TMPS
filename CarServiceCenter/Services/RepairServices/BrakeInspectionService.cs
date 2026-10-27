using CarServiceCenter.Interfaces;

namespace CarServiceCenter.Services.RepairServices;

/// <summary>
/// Brake system inspection. Sample price only, not a real workshop quotation.
/// </summary>
public class BrakeInspectionService : IRepairService
{
    public string Name => "Brake Inspection";
    public string Description => "Check pads, discs and brake fluid";
    public decimal Price => 300.00m;
    public int EstimatedDurationMinutes => 30;
}
