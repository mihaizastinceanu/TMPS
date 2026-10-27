using CarServiceCenter.Interfaces;
using CarServiceCenter.Models;
using CarServiceCenter.Services;
using CarServiceCenter.Services.RepairServices;

namespace CarServiceCenter.Tests;

public class WorkshopStatisticsServiceTests
{
    [Fact]
    public void Calculate_NoData_ReturnsZeros()
    {
        var vehicles = new VehicleService();
        var orders = new ServiceOrderService(vehicles);

        var stats = new WorkshopStatisticsService(vehicles, orders).Calculate();

        Assert.Equal(new WorkshopStatistics(0, 0, 0, 0, 0m, 0m), stats);
    }

    [Fact]
    public void Calculate_WithOrders_CountsStatusesAndValues()
    {
        var vehicles = new VehicleService();
        var orders = new ServiceOrderService(vehicles);
        vehicles.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);
        vehicles.RegisterVehicle("Maria Rusu", "Toyota", "Corolla", "K XY 777", 2021);

        var first = orders.CreateOrder(1, new IServicePriced[] { new OilChangeService(), new BrakeInspectionService() }); // 1150
        orders.CreateOrder(2, new IServicePriced[] { new EngineDiagnosticsService() }); // 450
        orders.CompleteOrder(first.Id);

        var stats = new WorkshopStatisticsService(vehicles, orders).Calculate();

        Assert.Equal(2, stats.RegisteredVehicles);
        Assert.Equal(2, stats.TotalOrders);
        Assert.Equal(1, stats.PendingOrders);
        Assert.Equal(1, stats.CompletedOrders);
        Assert.Equal(1600.00m, stats.TotalOrdersValue);
        Assert.Equal(1150.00m, stats.CompletedOrdersValue);
    }
}
