using CarServiceCreational.Domain.Models.Services;

namespace CarServiceCreational.Domain.Models;

/// <summary>
/// A work order for one vehicle. Created only through ServiceOrderBuilder.
/// </summary>
public class ServiceOrder
{
    public const decimal UrgentFeePercent = 20m;

    public int Number { get; }
    public Vehicle Vehicle { get; }
    public IReadOnlyList<RepairService> Services { get; }
    public IReadOnlyList<ServicePackage> Packages { get; }
    public string? Mechanic { get; }
    public string? Notes { get; }
    public bool IsUrgent { get; }

    internal ServiceOrder(int number, Vehicle vehicle, List<RepairService> services,
        List<ServicePackage> packages, string? mechanic, string? notes, bool isUrgent)
    {
        Number = number;
        Vehicle = vehicle;
        Services = services;
        Packages = packages;
        Mechanic = mechanic;
        Notes = notes;
        IsUrgent = isUrgent;
    }

    public decimal TotalCost
    {
        get
        {
            var total = Services.Sum(s => s.TotalCost) + Packages.Sum(p => p.TotalCost);
            return IsUrgent ? total * (1 + UrgentFeePercent / 100m) : total;
        }
    }
}
