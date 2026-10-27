using CarServiceCenter.Interfaces;
using CarServiceCenter.Services;
using CarServiceCenter.Services.RepairServices;

namespace CarServiceCenter.Tests;

public class VehicleRemovalServiceTests
{
    private readonly VehicleService _vehicleService = new();
    private readonly ServiceOrderService _orderService;
    private readonly VehicleRemovalService _removalService;

    public VehicleRemovalServiceTests()
    {
        _orderService = new ServiceOrderService(_vehicleService);
        _removalService = new VehicleRemovalService(_vehicleService, _orderService);
        _vehicleService.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "CLR794", 2018);
    }

    [Theory]
    [InlineData("CLR794")]
    [InlineData("clr 794")]
    public void Delete_VehicleWithoutOrders_IsRemoved(string registrationNumber)
    {
        var deleted = _removalService.DeleteByRegistrationNumber(registrationNumber);

        Assert.Equal("CLR794", deleted.RegistrationNumber);
        Assert.Empty(_vehicleService.GetAll());
    }

    [Fact]
    public void Delete_VehicleWithOnlyCompletedOrders_IsRemovedAndOrdersAreKept()
    {
        var order = _orderService.CreateOrder(1, new IServicePriced[] { new OilChangeService() });
        _orderService.CompleteOrder(order.Id);

        _removalService.DeleteByRegistrationNumber("clr794");

        Assert.Empty(_vehicleService.GetAll());
        Assert.Single(_orderService.GetAll());
        Assert.Equal("CLR794", _orderService.GetAll()[0].Vehicle.RegistrationNumber);
    }

    [Fact]
    public void Delete_VehicleWithPendingOrder_ThrowsAndKeepsVehicle()
    {
        var vehicle = _vehicleService.FindById(1)!;
        _orderService.CreateOrder(1, new IServicePriced[] { new OilChangeService() });

        Assert.False(_removalService.CanDelete(vehicle));
        Assert.Throws<InvalidOperationException>(() => _removalService.DeleteByRegistrationNumber("CLR 794"));
        Assert.Single(_vehicleService.GetAll());
    }

    [Fact]
    public void Delete_AfterPendingOrderIsCompleted_IsAllowed()
    {
        var vehicle = _vehicleService.FindById(1)!;
        var order = _orderService.CreateOrder(1, new IServicePriced[] { new OilChangeService() });

        _orderService.CompleteOrder(order.Id);

        Assert.True(_removalService.CanDelete(vehicle));
        _removalService.DeleteByRegistrationNumber("CLR794");
        Assert.Empty(_vehicleService.GetAll());
    }

    [Fact]
    public void Delete_UnknownRegistrationNumber_Throws()
    {
        Assert.Throws<ArgumentException>(() => _removalService.DeleteByRegistrationNumber("ABC 000"));
        Assert.Single(_vehicleService.GetAll());
    }

    [Fact]
    public void Delete_ThenRegisterSameNumberAgain_IsAllowedWithNewId()
    {
        _removalService.DeleteByRegistrationNumber("CLR794");

        var again = _vehicleService.RegisterVehicle("New Owner", "Dacia", "Logan", "clr 794", 2018);

        Assert.Equal(2, again.Id);
    }
}
