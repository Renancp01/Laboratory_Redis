namespace Infra.Redis.Interfaces.V2;

public interface IValidationStrategy<T>
{
    bool IsValid(T item, string key);
}