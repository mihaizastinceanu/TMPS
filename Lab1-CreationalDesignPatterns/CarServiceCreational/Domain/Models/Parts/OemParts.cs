namespace CarServiceCreational.Domain.Models.Parts;

/// <summary>
/// Original (OEM) parts, made by the car manufacturer's supplier.
/// </summary>
public class OemEngineOil : IEngineOil
{
    public string Name => "OEM Engine Oil 5W-30";
    public string Supplier => "Original manufacturer";
    public string Viscosity => "5W-30";
    public decimal Price => 720.00m;
}

public class OemBrakePads : IBrakePads
{
    public string Name => "OEM Brake Pads";
    public string Supplier => "Original manufacturer";
    public string Material => "Ceramic";
    public decimal Price => 1150.00m;
}
