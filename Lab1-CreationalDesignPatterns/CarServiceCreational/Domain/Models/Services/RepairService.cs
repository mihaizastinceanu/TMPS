using CarServiceCreational.Domain.Models.Parts;
using CarServiceCreational.Domain.Prototype;

namespace CarServiceCreational.Domain.Models.Services;

/// <summary>
/// Base class for every repair service: labor cost plus the parts it uses.
/// </summary>
public abstract class RepairService : IPrototype<RepairService>
{
    private List<IPart> _parts;

    public abstract string Name { get; }
    public abstract decimal LaborCost { get; }

    protected RepairService(IEnumerable<IPart> parts)
    {
        _parts = parts.ToList();
    }

    public IReadOnlyList<IPart> Parts => _parts;

    public decimal TotalCost => LaborCost + _parts.Sum(p => p.Price);

    /// <summary>
    /// Copies the service with its own parts list, so changing the copy does not change the original.
    /// </summary>
    public RepairService Clone()
    {
        var copy = (RepairService)MemberwiseClone();
        copy._parts = new List<IPart>(_parts);
        return copy;
    }
}
