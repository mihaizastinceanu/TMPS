using CarServiceCenter.Interfaces;

namespace CarServiceCenter.Services.RepairServices;

/// <summary>
/// Computer engine diagnostics. Sample price only, not a real workshop quotation.
/// </summary>
public class EngineDiagnosticsService : IRepairService
{
    public string Name => "Engine Diagnostics";
    public string Description => "Computer scan of engine error codes";
    public decimal Price => 450.00m;
    public int EstimatedDurationMinutes => 60;
}
