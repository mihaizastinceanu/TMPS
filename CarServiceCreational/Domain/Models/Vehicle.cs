namespace CarServiceCreational.Domain.Models;

/// <summary>
/// A vehicle brought to the workshop.
/// </summary>
public class Vehicle
{
    public string OwnerName { get; }
    public string Make { get; }
    public string Model { get; }
    public string RegistrationNumber { get; }
    public int Year { get; }

    public Vehicle(string ownerName, string make, string model, string registrationNumber, int year)
    {
        OwnerName = ownerName;
        Make = make;
        Model = model;
        RegistrationNumber = registrationNumber;
        Year = year;
    }

    public string DisplayName => $"{Make} {Model} ({RegistrationNumber})";
}
