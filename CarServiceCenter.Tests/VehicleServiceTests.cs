using CarServiceCenter.Services;

namespace CarServiceCenter.Tests;

public class VehicleServiceTests
{
    [Fact]
    public void RegisterVehicle_ValidData_AssignsUniqueIds()
    {
        var service = new VehicleService();

        var first = service.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);
        var second = service.RegisterVehicle("Maria Rusu", "Toyota", "Corolla", "K XY 777", 2021);

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
        Assert.Equal(2, service.GetAll().Count);
    }

    [Fact]
    public void RegisterVehicle_NormalizesRegistrationNumber()
    {
        var service = new VehicleService();

        var vehicle = service.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "  c   ab 123 ", 2018);

        Assert.Equal("C AB 123", vehicle.RegistrationNumber);
    }

    [Theory]
    [InlineData("C AB 123")]
    [InlineData("c ab 123")]
    [InlineData("CAB123")]
    public void RegisterVehicle_DuplicateRegistrationNumber_Throws(string duplicate)
    {
        var service = new VehicleService();
        service.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);

        Assert.Throws<InvalidOperationException>(
            () => service.RegisterVehicle("Other Owner", "BMW", "X5", duplicate, 2020));
        Assert.Single(service.GetAll());
    }

    [Theory]
    [InlineData("", "Dacia", "Logan", "C AB 123")]
    [InlineData("Ion", "  ", "Logan", "C AB 123")]
    [InlineData("Ion", "Dacia", "", "C AB 123")]
    [InlineData("Ion", "Dacia", "Logan", " ")]
    public void RegisterVehicle_MissingRequiredField_Throws(string owner, string make, string model, string number)
    {
        var service = new VehicleService();

        Assert.Throws<ArgumentException>(() => service.RegisterVehicle(owner, make, model, number, 2018));
    }

    [Theory]
    [InlineData(1900)]
    [InlineData(3000)]
    public void RegisterVehicle_YearOutOfRange_Throws(int year)
    {
        var service = new VehicleService();

        Assert.Throws<ArgumentException>(() => service.RegisterVehicle("Ion", "Dacia", "Logan", "C AB 123", year));
    }

    [Fact]
    public void GetAll_NoVehicles_ReturnsEmptyList()
    {
        Assert.Empty(new VehicleService().GetAll());
    }

    [Fact]
    public void FindById_UnknownId_ReturnsNull()
    {
        Assert.Null(new VehicleService().FindById(42));
    }

    [Theory]
    [InlineData("clr794")]
    [InlineData("CLR794")]
    [InlineData("CLR 794")]
    [InlineData("clr 794")]
    [InlineData("  Clr  794 ")]
    public void ToSearchKey_IgnoresCaseAndSpaces(string input)
    {
        Assert.Equal("CLR794", VehicleService.ToSearchKey(input));
    }

    [Theory]
    [InlineData("clr794")]
    [InlineData("CLR794")]
    [InlineData("CLR 794")]
    [InlineData("clr 794")]
    public void FindByRegistrationNumber_IgnoresCaseAndSpaces(string query)
    {
        var service = new VehicleService();
        var vehicle = service.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "CLR794", 2018);

        Assert.Same(vehicle, service.FindByRegistrationNumber(query));
    }

    [Fact]
    public void FindByRegistrationNumber_Unknown_ReturnsNull()
    {
        var service = new VehicleService();
        service.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "CLR794", 2018);

        Assert.Null(service.FindByRegistrationNumber("CLR795"));
    }

    [Fact]
    public void RegisterVehicle_DuplicateWrittenDifferently_Throws()
    {
        var service = new VehicleService();
        service.RegisterVehicle("Ion Popescu", "Dacia", "Logan", "clr794", 2018);

        Assert.Throws<InvalidOperationException>(
            () => service.RegisterVehicle("Other", "BMW", "X5", "CLR 794", 2020));
    }

    [Fact]
    public void SearchByRegistrationNumber_PartialText_ReturnsAllMatches()
    {
        var service = new VehicleService();
        service.RegisterVehicle("A", "Dacia", "Logan", "CLR 794", 2018);
        service.RegisterVehicle("B", "BMW", "X5", "CLR 100", 2020);
        service.RegisterVehicle("C", "Audi", "A4", "K XY 777", 2019);

        var matches = service.SearchByRegistrationNumber("clr");

        Assert.Equal(new[] { "CLR 794", "CLR 100" }, matches.Select(v => v.RegistrationNumber));
    }

    [Theory]
    [InlineData("zzz")]
    [InlineData("   ")]
    public void SearchByRegistrationNumber_NoMatchOrBlank_ReturnsEmpty(string text)
    {
        var service = new VehicleService();
        service.RegisterVehicle("A", "Dacia", "Logan", "CLR 794", 2018);

        Assert.Empty(service.SearchByRegistrationNumber(text));
    }

    [Fact]
    public void GetAllSortedByRegistrationNumber_SortsAToZIgnoringCaseAndSpaces()
    {
        var service = new VehicleService();
        service.RegisterVehicle("A", "Dacia", "Logan", "K XY 777", 2018);
        service.RegisterVehicle("B", "BMW", "X5", "clr 794", 2020);
        service.RegisterVehicle("C", "Audi", "A4", "B AA 001", 2019);

        var sorted = service.GetAllSortedByRegistrationNumber();

        Assert.Equal(new[] { "B AA 001", "CLR 794", "K XY 777" }, sorted.Select(v => v.RegistrationNumber));
        // The original registration order is not changed.
        Assert.Equal(new[] { 1, 2, 3 }, service.GetAll().Select(v => v.Id));
    }

    [Fact]
    public void RemoveVehicle_ExistingAndUnknownId()
    {
        var service = new VehicleService();
        service.RegisterVehicle("A", "Dacia", "Logan", "CLR794", 2018);

        Assert.True(service.RemoveVehicle(1));
        Assert.Empty(service.GetAll());
        Assert.False(service.RemoveVehicle(1));
    }
}
