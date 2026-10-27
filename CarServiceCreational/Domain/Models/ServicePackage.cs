using CarServiceCreational.Domain.Models.Services;
using CarServiceCreational.Domain.Prototype;

namespace CarServiceCreational.Domain.Models;

/// <summary>
/// A named group of services sold together with a discount, e.g. "Basic Maintenance".
/// Packages are cloned from templates instead of being built from scratch every time.
/// </summary>
public class ServicePackage : IPrototype<ServicePackage>
{
    private List<RepairService> _services;

    public string Name { get; set; }
    public decimal DiscountPercent { get; set; }

    public ServicePackage(string name, decimal discountPercent, IEnumerable<RepairService> services)
    {
        Name = name;
        DiscountPercent = discountPercent;
        _services = services.ToList();
    }

    public IReadOnlyList<RepairService> Services => _services;

    public decimal TotalCost => _services.Sum(s => s.TotalCost) * (1 - DiscountPercent / 100m);

    public void AddService(RepairService service) => _services.Add(service);

    /// <summary>
    /// Deep copy: every service inside the package is cloned too.
    /// </summary>
    public ServicePackage Clone()
    {
        var copy = (ServicePackage)MemberwiseClone();
        copy._services = _services.Select(s => s.Clone()).ToList();
        return copy;
    }
}
