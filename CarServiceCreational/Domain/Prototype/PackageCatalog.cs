using CarServiceCreational.Domain.Factory.PartsFactories;
using CarServiceCreational.Domain.Factory.ServiceCreators;
using CarServiceCreational.Domain.Models;

namespace CarServiceCreational.Domain.Prototype;

/// <summary>
/// Keeps ready-made package templates and gives out copies of them.
/// The templates themselves are never handed out, so they cannot be changed by mistake.
/// </summary>
public class PackageCatalog
{
    private readonly Dictionary<string, ServicePackage> _templates = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> PackageNames => _templates.Keys;

    public void AddTemplate(ServicePackage template) => _templates[template.Name] = template;

    public ServicePackage Get(string name)
    {
        if (!_templates.TryGetValue(name, out var template))
            throw new KeyNotFoundException($"Package '{name}' does not exist.");

        return template.Clone();
    }

    public static PackageCatalog CreateDefault(IPartsFactory parts)
    {
        var catalog = new PackageCatalog();

        catalog.AddTemplate(new ServicePackage("Basic Maintenance", 10m, new[]
        {
            new OilChangeCreator().PrepareService(parts),
            new EngineDiagnosticsCreator().PrepareService(parts)
        }));

        catalog.AddTemplate(new ServicePackage("Full Check", 15m, new[]
        {
            new OilChangeCreator().PrepareService(parts),
            new BrakeReplacementCreator().PrepareService(parts),
            new EngineDiagnosticsCreator().PrepareService(parts)
        }));

        return catalog;
    }
}
