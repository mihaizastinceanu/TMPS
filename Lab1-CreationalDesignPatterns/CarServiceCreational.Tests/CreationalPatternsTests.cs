using CarServiceCreational.Domain.Builder;
using CarServiceCreational.Domain.Factory.PartsFactories;
using CarServiceCreational.Domain.Factory.ServiceCreators;
using CarServiceCreational.Domain.Models;
using CarServiceCreational.Domain.Models.Parts;
using CarServiceCreational.Domain.Models.Services;
using CarServiceCreational.Domain.Prototype;
using CarServiceCreational.Domain.Registry;

namespace CarServiceCreational.Tests;

public class CreationalPatternsTests
{
    private static readonly Vehicle Logan = new("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);

    [Fact]
    public void Singleton_ReturnsSameInstance()
    {
        Assert.Same(WorkshopRegistry.Instance, WorkshopRegistry.Instance);
    }

    [Fact]
    public void Singleton_OrderNumbersAreUnique()
    {
        var first = new ServiceOrderBuilder().ForVehicle(Logan).AddService(new EngineDiagnostics()).Build();
        var second = new ServiceOrderBuilder().ForVehicle(Logan).AddService(new EngineDiagnostics()).Build();

        Assert.True(second.Number > first.Number);
        Assert.Contains(first, WorkshopRegistry.Instance.Orders);
        Assert.Contains(second, WorkshopRegistry.Instance.Orders);
    }

    [Fact]
    public void AbstractFactory_CreatesPartsOfTheSameTier()
    {
        IPartsFactory oem = new OemPartsFactory();
        IPartsFactory aftermarket = new AftermarketPartsFactory();

        Assert.IsType<OemEngineOil>(oem.CreateEngineOil());
        Assert.IsType<OemBrakePads>(oem.CreateBrakePads());
        Assert.IsType<AftermarketEngineOil>(aftermarket.CreateEngineOil());
        Assert.IsType<AftermarketBrakePads>(aftermarket.CreateBrakePads());
    }

    [Fact]
    public void FactoryMethod_CreatesCorrectServiceWithParts()
    {
        var service = new OilChangeCreator().PrepareService(new AftermarketPartsFactory());

        Assert.IsType<OilChange>(service);
        Assert.IsType<AftermarketEngineOil>(Assert.Single(service.Parts));
        Assert.Equal(300.00m + 430.00m, service.TotalCost);
    }

    [Fact]
    public void Prototype_ChangingCopyDoesNotChangeTemplate()
    {
        var catalog = PackageCatalog.CreateDefault(new OemPartsFactory());

        var copy = catalog.Get("Basic Maintenance");
        copy.Name = "Changed";
        copy.AddService(new EngineDiagnostics());

        var fresh = catalog.Get("Basic Maintenance");
        Assert.Equal("Basic Maintenance", fresh.Name);
        Assert.Equal(2, fresh.Services.Count);
    }

    [Fact]
    public void Prototype_CloneIsDeepCopy()
    {
        var package = new ServicePackage("Test", 0m, new[] { new EngineDiagnostics() });

        var copy = package.Clone();

        Assert.NotSame(package, copy);
        Assert.NotSame(package.Services[0], copy.Services[0]);
        Assert.Equal(package.TotalCost, copy.TotalCost);
    }

    [Fact]
    public void Prototype_UnknownPackage_Throws()
    {
        var catalog = PackageCatalog.CreateDefault(new OemPartsFactory());

        Assert.Throws<KeyNotFoundException>(() => catalog.Get("Does not exist"));
    }

    [Fact]
    public void Builder_SetsAllOptionalFields()
    {
        var order = new ServiceOrderBuilder()
            .ForVehicle(Logan)
            .AddService(new EngineDiagnostics())
            .AssignMechanic("Andrei")
            .WithNotes("Check engine light")
            .MarkUrgent()
            .Build();

        Assert.Same(Logan, order.Vehicle);
        Assert.Equal("Andrei", order.Mechanic);
        Assert.Equal("Check engine light", order.Notes);
        Assert.True(order.IsUrgent);
        Assert.Equal(400.00m * 1.2m, order.TotalCost);
    }

    [Fact]
    public void Builder_PackageDiscountIsApplied()
    {
        var package = new ServicePackage("Promo", 10m, new[] { new EngineDiagnostics() });

        var order = new ServiceOrderBuilder().ForVehicle(Logan).AddPackage(package).Build();

        Assert.Equal(360.00m, order.TotalCost);
    }

    [Fact]
    public void Builder_WithoutVehicle_Throws()
    {
        var builder = new ServiceOrderBuilder().AddService(new EngineDiagnostics());

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Builder_WithoutServices_Throws()
    {
        var builder = new ServiceOrderBuilder().ForVehicle(Logan);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }
}
