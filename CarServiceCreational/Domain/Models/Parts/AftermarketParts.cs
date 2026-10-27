namespace CarServiceCreational.Domain.Models.Parts;

/// <summary>
/// Cheaper parts from third-party producers.
/// </summary>
public class AftermarketEngineOil : IEngineOil
{
    public string Name => "Aftermarket Engine Oil 10W-40";
    public string Supplier => "Third-party producer";
    public string Viscosity => "10W-40";
    public decimal Price => 430.00m;
}

public class AftermarketBrakePads : IBrakePads
{
    public string Name => "Aftermarket Brake Pads";
    public string Supplier => "Third-party producer";
    public string Material => "Semi-metallic";
    public decimal Price => 640.00m;
}
