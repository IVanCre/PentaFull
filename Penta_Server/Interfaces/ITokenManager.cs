namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Сложный crud для работы с токенами доступа
    /// </summary>
    public interface ITokenManager
    {
        public string[] CreateTokenPack(int userID, string username, string pass);
        public string[] RefreshJwtToken(string refreshToken);
        public int FindUserByToken(string token);
    }
}
