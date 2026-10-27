using CarServiceCenter.Interfaces;
using CarServiceCenter.Services;
using CarServiceCenter.Services.RepairServices;
using CarServiceCenter.UI;

// Composition root: create the objects, connect them and start the menu.
// To offer a new repair service, add one line to this list (Open/Closed Principle).
var catalog = new ServiceCatalog(new List<IRepairService>
{
    new OilChangeService(),
    new BrakeInspectionService(),
    new EngineDiagnosticsService(),
    new TireReplacementService(),
    new BatteryReplacementService()
});

var vehicleService = new VehicleService();
var orderService = new ServiceOrderService(vehicleService);
var removalService = new VehicleRemovalService(vehicleService, orderService);
var statisticsService = new WorkshopStatisticsService(vehicleService, orderService);

var menu = new ConsoleMenu(
    new InputReader(), vehicleService, orderService, removalService, statisticsService, catalog);
menu.Run();
