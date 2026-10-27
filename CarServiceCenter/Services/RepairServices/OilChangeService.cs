using CarServiceCenter.Interfaces;

namespace CarServiceCenter.Services.RepairServices;

/// <summary>
/// Oil and filter change. Sample price only, not a real workshop quotation.
/// </summary>
public class OilChangeService : IRepairService
{
    public string Name => "Oil and Filter Change";
    public string Description => "Replace engine oil and oil filter";
    public decimal Price => 850.00m;
    public int EstimatedDurationMinutes => 45;
}
