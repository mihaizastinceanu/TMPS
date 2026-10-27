using CarServiceCreational.Domain.Factory.PartsFactories;
using CarServiceCreational.Domain.Models.Services;

namespace CarServiceCreational.Domain.Factory.ServiceCreators;

/// <summary>
/// Factory Method: subclasses decide which RepairService gets created.
/// PrepareService holds the common logic and calls the factory method.
/// </summary>
public abstract class RepairServiceCreator
{
    protected abstract RepairService CreateService(IPartsFactory parts);

    public RepairService PrepareService(IPartsFactory parts)
    {
        var service = CreateService(parts);

        if (service.TotalCost <= 0)
            throw new InvalidOperationException($"Service '{service.Name}' has an invalid cost.");

        return service;
    }
}

public class OilChangeCreator : RepairServiceCreator
{
    protected override RepairService CreateService(IPartsFactory parts) => new OilChange(parts.CreateEngineOil());
}

public class BrakeReplacementCreator : RepairServiceCreator
{
    protected override RepairService CreateService(IPartsFactory parts) => new BrakeReplacement(parts.CreateBrakePads());
}

public class EngineDiagnosticsCreator : RepairServiceCreator
{
    protected override RepairService CreateService(IPartsFactory parts) => new EngineDiagnostics();
}
