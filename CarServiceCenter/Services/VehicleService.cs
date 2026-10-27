using CarServiceCenter.Models;

namespace CarServiceCenter.Services;

/// <summary>
/// Handles vehicle operations: registration (with validation), lookup, search, sorting and removal.
/// Vehicles are stored in memory.
/// </summary>
public class VehicleService
{
    public const int MinimumYear = 1950;

    private readonly List<Vehicle> _vehicles = new();
    private int _nextId = 1;

    /// <summary>
    /// The latest manufacturing year accepted (next year's models can already be on the road).
    /// </summary>
    public static int MaximumYear => DateTime.Now.Year + 1;

    public IReadOnlyList<Vehicle> GetAll() => _vehicles;

    public Vehicle? FindById(int id) => _vehicles.FirstOrDefault(vehicle => vehicle.Id == id);

    /// <summary>
    /// Finds the vehicle with exactly this registration number, ignoring case and spaces
    /// ("clr794", "CLR 794" and "clr 794" all find "CLR794"). Returns null if there is none.
    /// </summary>
    public Vehicle? FindByRegistrationNumber(string registrationNumber)
    {
        string key = ToSearchKey(registrationNumber);
        return _vehicles.FirstOrDefault(vehicle => ToSearchKey(vehicle.RegistrationNumber) == key);
    }

    /// <summary>
    /// Returns all vehicles whose registration number contains the given text, ignoring case and spaces.
    /// "clr" finds "CLR794" and "CLR 100"; a full number finds that vehicle.
    /// </summary>
    public IReadOnlyList<Vehicle> SearchByRegistrationNumber(string text)
    {
        string key = ToSearchKey(text);
        if (key.Length == 0)
        {
            return new List<Vehicle>();
        }

        return _vehicles
            .Where(vehicle => ToSearchKey(vehicle.RegistrationNumber).Contains(key))
            .ToList();
    }

    /// <summary>
    /// Returns all vehicles sorted A-Z by registration number (case and spaces ignored).
    /// </summary>
    public IReadOnlyList<Vehicle> GetAllSortedByRegistrationNumber()
    {
        return _vehicles
            .OrderBy(vehicle => ToSearchKey(vehicle.RegistrationNumber), StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Removes the vehicle with the given ID from the register. Returns false if it does not exist.
    /// Business checks (pending orders) are done by VehicleRemovalService before calling this.
    /// </summary>
    public bool RemoveVehicle(int id)
    {
        Vehicle? vehicle = FindById(id);
        return vehicle is not null && _vehicles.Remove(vehicle);
    }

    /// <summary>
    /// Validates the data, checks the registration number is unique and stores a new vehicle.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a field is empty or the year is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the registration number already exists.</exception>
    public Vehicle RegisterVehicle(string ownerName, string make, string model, string registrationNumber, int year)
    {
        RequireText(ownerName, "Owner name");
        RequireText(make, "Make");
        RequireText(model, "Model");
        RequireText(registrationNumber, "Registration number");

        if (year < MinimumYear || year > MaximumYear)
        {
            throw new ArgumentException($"Year must be between {MinimumYear} and {MaximumYear}.");
        }

        string normalizedNumber = NormalizeRegistrationNumber(registrationNumber);
        Vehicle? existing = FindByRegistrationNumber(normalizedNumber);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"A vehicle with registration number '{existing.RegistrationNumber}' is already registered " +
                $"(ID {existing.Id}).");
        }

        var vehicle = new Vehicle(_nextId++, ownerName.Trim(), make.Trim(), model.Trim(), normalizedNumber, year);
        _vehicles.Add(vehicle);
        return vehicle;
    }

    /// <summary>
    /// Converts a registration number to the form used for comparing: upper case, no spaces.
    /// "clr 794", "CLR 794" and "clr794" all become "CLR794".
    /// </summary>
    public static string ToSearchKey(string registrationNumber)
    {
        return new string(registrationNumber
            .Where(character => !char.IsWhiteSpace(character))
            .ToArray())
            .ToUpperInvariant();
    }

    /// <summary>
    /// Upper-cases the number and collapses repeated spaces, e.g. " c  ab 123 " becomes "C AB 123".
    /// </summary>
    private static string NormalizeRegistrationNumber(string registrationNumber)
    {
        string[] parts = registrationNumber.Trim().ToUpperInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static void RequireText(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.");
        }
    }
}
