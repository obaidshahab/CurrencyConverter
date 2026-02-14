namespace CurrencyConverter.Utilities
{
    public interface IJwtService
    {
        string GenerateToken(string clientId, string username, string role);
    }
}
