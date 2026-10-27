using CarServiceCenter.Interfaces;

namespace CarServiceCenter.Models;

/// <summary>
/// Represents a service order: one vehicle, the services selected for it, a creation date and a status.
/// </summary>
public class ServiceOrder : IStatusManageable
{
    private readonly List<IServicePriced> _services;

    public int Id { get; }
    public Vehicle Vehicle { get; }
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// The services included in this order. Only their name and price are needed here.
    /// </summary>
    public IReadOnlyList<IServicePriced> Services => _services;

    public ServiceOrder(int id, Vehicle vehicle, IEnumerable<IServicePriced> services, DateTime createdAt)
    {
        Id = id;
        Vehicle = vehicle;
        _services = services.ToList();
        CreatedAt = createdAt;
        Status = OrderStatus.Pending;
    }

    /// <summary>
    /// Calculates the order total by asking every service for its own price (polymorphism).
    /// </summary>
    public decimal TotalCost => _services.Sum(service => service.Price);

    /// <summary>
    /// Changes the status from Pending to Completed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the order is already completed.</exception>
    public void MarkCompleted()
    {
        if (Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException($"Order #{Id} is already completed.");
        }

        Status = OrderStatus.Completed;
    }
}
