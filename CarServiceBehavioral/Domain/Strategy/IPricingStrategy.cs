using CarServiceBehavioral.Domain.Models;

namespace CarServiceBehavioral.Domain.Strategy;

/// <summary>
/// Strategy: a way of calculating the customer's discount for an order.
/// </summary>
public interface IPricingStrategy
{
    string Name { get; }
    decimal CalculateDiscount(ServiceOrder order);
}

/// <summary>A customer without any card or contract pays the full price.</summary>
public class RegularPricing : IPricingStrategy
{
    public string Name => "Regular";
    public decimal CalculateDiscount(ServiceOrder order) => 0m;
}

/// <summary>A loyalty card holder gets a percent off the whole order (10% by default).</summary>
public class LoyaltyCardPricing : IPricingStrategy
{
    private readonly decimal _percent;

    public LoyaltyCardPricing(decimal percent = 10m)
    {
        if (percent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percent));
        _percent = percent;
    }

    public string Name => $"Loyalty card (-{_percent}%)";
    public decimal CalculateDiscount(ServiceOrder order) => order.TotalCost * _percent / 100m;
}

/// <summary>
/// Companies with a fleet contract do not pay the urgent fee and get 5% off the rest.
/// </summary>
public class FleetContractPricing : IPricingStrategy
{
    public const decimal ContractPercent = 5m;

    public string Name => $"Fleet contract (no urgent fee, -{ContractPercent}%)";
    public decimal CalculateDiscount(ServiceOrder order) => order.UrgentFee + order.BaseCost * ContractPercent / 100m;
}
