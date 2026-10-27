using CarServiceCreational.Domain.Models.Parts;

namespace CarServiceCreational.Domain.Models.Services;

public class OilChange : RepairService
{
    public OilChange(IEngineOil oil) : base(new IPart[] { oil }) { }

    public override string Name => "Oil Change";
    public override decimal LaborCost => 300.00m;
}

public class BrakeReplacement : RepairService
{
    public BrakeReplacement(IBrakePads pads) : base(new IPart[] { pads }) { }

    public override string Name => "Brake Pads Replacement";
    public override decimal LaborCost => 500.00m;
}

/// <summary>
/// Computer diagnostics. Labor only, no parts.
/// </summary>
public class EngineDiagnostics : RepairService
{
    public EngineDiagnostics() : base(Array.Empty<IPart>()) { }

    public override string Name => "Engine Diagnostics";
    public override decimal LaborCost => 400.00m;
}
