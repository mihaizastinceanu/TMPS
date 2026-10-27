namespace CarServiceCenter.Models;

/// <summary>
/// A snapshot of the workshop numbers produced by WorkshopStatisticsService.
/// </summary>
/// <param name="TotalOrdersValue">Value of all orders. This is NOT money received, because payments are not tracked.</param>
/// <param name="CompletedOrdersValue">Value of the work that has been completed (still not necessarily paid).</param>
public record WorkshopStatistics(
    int RegisteredVehicles,
    int TotalOrders,
    int PendingOrders,
    int CompletedOrders,
    decimal TotalOrdersValue,
    decimal CompletedOrdersValue);
