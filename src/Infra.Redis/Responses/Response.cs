namespace Infra.Redis.Responses;

public class Response<T>
{
    public DateTime ExpireAt { get; set; }
    
    public T? Data { get; set; }

    public bool Expired()
    {
        return DateTime.UtcNow > ExpireAt;
    }
}