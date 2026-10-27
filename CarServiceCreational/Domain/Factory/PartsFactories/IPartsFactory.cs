using CarServiceCreational.Domain.Models.Parts;

namespace CarServiceCreational.Domain.Factory.PartsFactories;

/// <summary>
/// Abstract Factory: creates a whole family of parts of the same quality tier,
/// so OEM and aftermarket parts are never mixed in one service.
/// </summary>
public interface IPartsFactory
{
    string TierName { get; }
    IEngineOil CreateEngineOil();
    IBrakePads CreateBrakePads();
}

public class OemPartsFactory : IPartsFactory
{
    public string TierName => "OEM";
    public IEngineOil CreateEngineOil() => new OemEngineOil();
    public IBrakePads CreateBrakePads() => new OemBrakePads();
}

public class AftermarketPartsFactory : IPartsFactory
{
    public string TierName => "Aftermarket";
    public IEngineOil CreateEngineOil() => new AftermarketEngineOil();
    public IBrakePads CreateBrakePads() => new AftermarketBrakePads();
}
