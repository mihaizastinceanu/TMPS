namespace CarServiceCenter.Interfaces;

/// <summary>
/// A repair service offered by the workshop. Adds catalog information
/// (description and estimated duration) on top of the pricing capability.
/// New services are added by implementing this interface (Open/Closed Principle).
/// </summary>
public interface IRepairService : IServicePriced
{
    string Description { get; }
    int EstimatedDurationMinutes { get; }
}
