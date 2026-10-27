namespace CarServiceCreational.Domain.Models.Parts;

/// <summary>
/// A spare part used during a repair. Sample prices only.
/// </summary>
public interface IPart
{
    string Name { get; }
    string Supplier { get; }
    decimal Price { get; }
}

public interface IEngineOil : IPart
{
    string Viscosity { get; }
}

public interface IBrakePads : IPart
{
    string Material { get; }
}
