using CarServiceCenter.Interfaces;

namespace CarServiceCenter.Services;

/// <summary>
/// Holds the list of repair services the workshop offers.
/// The services are passed in from Program.cs, so this class never names a concrete service.
/// </summary>
public class ServiceCatalog
{
    private readonly List<IRepairService> _services;

    public ServiceCatalog(IEnumerable<IRepairService> services)
    {
        _services = services.ToList();
    }

    public IReadOnlyList<IRepairService> Services => _services;

    /// <summary>
    /// Returns the service shown at the given menu number (1-based), or null if the number is invalid.
    /// </summary>
    public IRepairService? GetByNumber(int number)
    {
        if (number < 1 || number > _services.Count)
        {
            return null;
        }

        return _services[number - 1];
    }
}
