namespace Coontrera.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string email, string firebaseUid);
    }
}