using CarServiceCreational.Domain.Models;

namespace CarServiceCreational.Domain.Pool;

/// <summary>
/// Object Pool: the workshop has a fixed number of repair bays (3 by default).
/// A car takes a free bay and gives it back when the repair is done.
/// If all bays are busy, the next car waits until one is released.
/// </summary>
public class RepairBayPool
{
    public const int DefaultCapacity = 3;

    private readonly Queue<RepairBay> _freeBays = new();
    private readonly HashSet<RepairBay> _allBays = new();
    private readonly SemaphoreSlim _available;
    private readonly object _lock = new();

    public RepairBayPool(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "The pool needs at least one bay.");

        for (var number = 1; number <= capacity; number++)
        {
            var bay = new RepairBay(number);
            _allBays.Add(bay);
            _freeBays.Enqueue(bay);
        }

        Capacity = capacity;
        _available = new SemaphoreSlim(capacity, capacity);
    }

    public int Capacity { get; }

    public int FreeBays
    {
        get { lock (_lock) return _freeBays.Count; }
    }

    /// <summary>
    /// Takes a free bay for the vehicle. Blocks while all bays are occupied.
    /// </summary>
    public RepairBay Acquire(Vehicle vehicle)
    {
        _available.Wait();
        return TakeBay(vehicle);
    }

    /// <summary>
    /// Same as Acquire, but the car waits without blocking the thread.
    /// </summary>
    public async Task<RepairBay> AcquireAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        await _available.WaitAsync(cancellationToken);
        return TakeBay(vehicle);
    }

    /// <summary>
    /// Returns the bay to the pool, so the next waiting car can use it.
    /// </summary>
    public void Release(RepairBay bay)
    {
        lock (_lock)
        {
            if (!_allBays.Contains(bay))
                throw new InvalidOperationException($"Bay {bay.Number} does not belong to this workshop.");
            if (!bay.IsOccupied)
                throw new InvalidOperationException($"Bay {bay.Number} is already free.");

            bay.Reset();
            _freeBays.Enqueue(bay);
        }

        _available.Release();
    }

    private RepairBay TakeBay(Vehicle vehicle)
    {
        lock (_lock)
        {
            var bay = _freeBays.Dequeue();
            bay.Occupy(vehicle);
            return bay;
        }
    }
}
