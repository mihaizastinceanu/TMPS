namespace CarServiceCenter.Interfaces;

/// <summary>
/// The pricing capability of a service: a name and a price.
/// This is all that ServiceOrder needs to build a cost breakdown and calculate a total.
/// </summary>
public interface IServicePriced
{
    string Name { get; }
    decimal Price { get; }
}
