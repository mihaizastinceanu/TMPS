namespace CarServiceCenter.Models;

/// <summary>
/// Represents a vehicle registered in the workshop. It only stores vehicle data.
/// </summary>
public class Vehicle
{
    public int Id { get; }
    public string OwnerName { get; }
    public string Make { get; }
    public string Model { get; }
    public string RegistrationNumber { get; }
    public int Year { get; }

    public Vehicle(int id, string ownerName, string make, string model, string registrationNumber, int year)
    {
        Id = id;
        OwnerName = ownerName;
        Make = make;
        Model = model;
        RegistrationNumber = registrationNumber;
        Year = year;
    }

    /// <summary>
    /// Returns a short, human-readable description, e.g. "Dacia Logan (C AB 123)".
    /// </summary>
    public string DisplayName => $"{Make} {Model} ({RegistrationNumber})";
}
