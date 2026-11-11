namespace CarServiceBehavioral.Domain.Models;

/// <summary>
/// What the customer pays for an order. Created by the Cashier with a pricing strategy.
/// </summary>
public record Invoice(int OrderNumber, string PricingName, decimal Subtotal, decimal Discount)
{
    public decimal Total => Subtotal - Discount;
}
