using CarServiceCenter.Interfaces;
using CarServiceCenter.Services;
using CarServiceCenter.Services.RepairServices;

namespace CarServiceCenter.Tests;

public class ServiceCatalogTests
{
    private readonly ServiceCatalog _catalog = new(new IRepairService[]
    {
        new OilChangeService(),
        new BrakeInspectionService()
    });

    [Fact]
    public void GetByNumber_ValidNumber_ReturnsService()
    {
        Assert.Equal("Brake Inspection", _catalog.GetByNumber(2)?.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void GetByNumber_InvalidNumber_ReturnsNull(int number)
    {
        Assert.Null(_catalog.GetByNumber(number));
    }
}
