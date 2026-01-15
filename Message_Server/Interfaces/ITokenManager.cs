namespace Message_Server.Interfaces
{
    public interface ITokenManager
    {
        public string CreateToken(int userID, string username, string pass);
        public string GetToken( string username, string pass);
        public string CreateRefreshToken();
        public int FindUserByToken(string token);
    }
}
