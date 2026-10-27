using CarServiceCenter.Interfaces;
using CarServiceCenter.Models;
using CarServiceCenter.Services;
using CarServiceCenter.Services.RepairServices;

namespace CarServiceCenter.Tests;

public class ServiceOrderServiceTests
{
    private readonly VehicleService _vehicleService = new();
    private readonly ServiceOrderService _orderService;

    public ServiceOrderServiceTests()
    {
        _orderService = new ServiceOrderService(_vehicleService);
        _vehicleService.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);
    }

    [Fact]
    public void CreateOrder_MultipleServices_CalculatesCorrectTotal()
    {
        var order = _orderService.CreateOrder(1, new IServicePriced[]
        {
            new OilChangeService(),       // 850.00
            new BrakeInspectionService(), // 300.00
            new EngineDiagnosticsService() // 450.00
        });

        Assert.Equal(3, order.Services.Count);
        Assert.Equal(1600.00m, order.TotalCost);
    }

    [Fact]
    public void CreateOrder_NewOrder_IsPendingAndLinkedToVehicle()
    {
        var order = _orderService.CreateOrder(1, new IServicePriced[] { new TireReplacementService() });

        Assert.Equal(1, order.Id);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal("C AB 123", order.Vehicle.RegistrationNumber);
        Assert.Single(_orderService.GetAll());
    }

    [Fact]
    public void CreateOrder_InvalidVehicleId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => _orderService.CreateOrder(99, new IServicePriced[] { new OilChangeService() }));
        Assert.Empty(_orderService.GetAll());
    }

    [Fact]
    public void CreateOrder_NoServices_Throws()
    {
        Assert.Throws<ArgumentException>(() => _orderService.CreateOrder(1, Array.Empty<IServicePriced>()));
        Assert.Empty(_orderService.GetAll());
    }

    [Fact]
    public void CompleteOrder_PendingOrder_BecomesCompleted()
    {
        var order = _orderService.CreateOrder(1, new IServicePriced[] { new OilChangeService() });

        _orderService.CompleteOrder(order.Id);

        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void CompleteOrder_AlreadyCompleted_Throws()
    {
        var order = _orderService.CreateOrder(1, new IServicePriced[] { new OilChangeService() });
        _orderService.CompleteOrder(order.Id);

        Assert.Throws<InvalidOperationException>(() => _orderService.CompleteOrder(order.Id));
    }

    [Fact]
    public void CompleteOrder_UnknownOrder_Throws()
    {
        Assert.Throws<ArgumentException>(() => _orderService.CompleteOrder(5));
    }

    [Fact]
    public void GetAll_NoOrders_ReturnsEmptyList()
    {
        Assert.Empty(_orderService.GetAll());
    }

    /// <summary>
    /// A service that exists only in the tests. The order logic prices it correctly
    /// without any change to ServiceOrder or ServiceOrderService (Open/Closed Principle).
    /// </summary>
    private class WheelAlignmentService : IRepairService
    {
        public string Name => "Wheel Alignment";
        public string Description => "Test-only service";
        public decimal Price => 275.50m;
        public int EstimatedDurationMinutes => 40;
    }

    [Fact]
    public void CreateOrder_NewServiceType_IsPricedWithoutChangingOrderCode()
    {
        var order = _orderService.CreateOrder(1, new IServicePriced[]
        {
            new WheelAlignmentService(),
            new BatteryReplacementService() // 350.00
        });

        Assert.Equal(625.50m, order.TotalCost);
    }
}
