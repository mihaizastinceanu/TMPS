namespace CarServiceCreational.Domain.Prototype;

/// <summary>
/// Prototype pattern: an object that knows how to copy itself.
/// </summary>
public interface IPrototype<out T>
{
    T Clone();
}
