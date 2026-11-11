using CarServiceBehavioral.Domain.Models;

namespace CarServiceBehavioral.Domain.Strategy;

/// <summary>
/// Strategy context: the cashier does not know how discounts are calculated,
/// it only asks the current strategy. The strategy can be changed for every customer.
/// </summary>
public class Cashier
{
    private IPricingStrategy _strategy;

    public Cashier(IPricingStrategy strategy)
    {
        _strategy = strategy;
    }

    public IPricingStrategy Strategy => _strategy;

    public void SetStrategy(IPricingStrategy strategy) => _strategy = strategy;

    /// <summary>
    /// Creates the invoice with the current strategy and hands the car over to the customer.
    /// The order's state decides if this is allowed (only when the car is ready for pickup).
    /// </summary>
    public Invoice Checkout(ServiceOrder order)
    {
        var subtotal = Math.Round(order.TotalCost, 2);
        var discount = Math.Round(_strategy.CalculateDiscount(order), 2);
        var invoice = new Invoice(order.Number, _strategy.Name, subtotal, discount);

        order.Deliver(invoice);
        return invoice;
    }
}
