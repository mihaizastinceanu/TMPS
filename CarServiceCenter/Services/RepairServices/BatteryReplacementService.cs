using CarServiceCenter.Interfaces;

namespace CarServiceCenter.Services.RepairServices;

/// <summary>
/// Battery replacement (labour only). Sample price only, not a real workshop quotation.
/// Added after the first four services to demonstrate the Open/Closed Principle:
/// only this new class and one line in Program.cs were needed; no pricing or order code changed.
/// </summary>
public class BatteryReplacementService : IRepairService
{
    public string Name => "Battery Replacement";
    public string Description => "Remove old battery, fit and test new one (labour)";
    public decimal Price => 350.00m;
    public int EstimatedDurationMinutes => 20;
}
