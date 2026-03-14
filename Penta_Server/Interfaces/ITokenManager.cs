namespace Penta_Server.Interfaces
{
    public interface ITokenManager
    {
        public string[] CreateTokenPack(int userID, string username, string pass);
        public string[] RefreshJwtToken(string refreshToken);
        public int FindUserByToken(string token);
        public string GetTokenHash(string token);
    }
}
