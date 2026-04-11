

using Penta_ClientLib.Repository;
using Penta_ClientLib.Services;
using System.IdentityModel.Tokens.Jwt;


namespace Penta_ClientLib.Interfaces
{
    /// <summary>
    /// Единая точка доступа к настройкам
    /// </summary>
    public interface ISettingsProvider
    {
        Task<int> GetUserID();
        Task SetUserID(int value);
        Task<string> GetCurrentUserContactID();


        Task<string> GetUserLogin();
        Task SetUserLogin(string value);


        Task<string> GetUserPassword();
        Task SetUserPassword(string value);


        Task<string> GetAccessToken();
        Task SetAccessToken(string value);


        Task<string> GetRefreshToken();
        Task SetRefreshToken(string value);

        Task<string> GetServerURL();
        Task SetServerURL(string value);

        TimeSpan GetLifetimeSecondsLeft(string token);
    }


    internal class SettingsProvider(ISettingsHolder db):ISettingsProvider
    {
        private ISettingsHolder _db = db;


        private int currUserID = -1;
        public async Task<int> GetUserID()
        {
            if (currUserID == -1)
            {
                var result=await _db.GetValueByName<int>("userID");
                if (result > 0)
                    currUserID = result;
            }

            return currUserID;
        }
        public Task SetUserID(int value)
        {
            currUserID = value;
            return _db.SetValueByName("userID", currUserID);
        }

        public async Task<string> GetCurrentUserContactID()
        {
            if (await GetUserID() > 0)
                return ContactConverter.ConvertUserIDToContactID(currUserID);
            else
                return string.Empty;
        }



        public Task<string> GetUserLogin() => _db.GetValueByName<string>("userLogin"); 
        public Task SetUserLogin(string value)=> _db.SetValueByName("userLogin",value);


        public  Task<string> GetUserPassword() => _db.GetValueByName<string>("userPass");
        public Task SetUserPassword(string value)=> _db.SetValueByName("userPass", value);


        public Task<string> GetAccessToken() => _db.GetValueByName<string>("accessToken");
        public Task SetAccessToken(string value)=>_db.SetValueByName("accessToken", value);


        public Task<string> GetRefreshToken() => _db.GetValueByName<string>("refreshToken");
        public Task SetRefreshToken(string value)=> _db.SetValueByName("refreshToken", value);

        public Task<string> GetServerURL() => _db.GetValueByName<string>("serverURL");
        public Task SetServerURL(string value) => _db.SetValueByName("serverURL", value);


        /// <summary>
        /// Сколько секунд до истечения срока действия токена осталось
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public TimeSpan GetLifetimeSecondsLeft(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
                var expClaim = jwtToken.Payload.Expiration;
                if (expClaim.HasValue)
                {
                    var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var remainingSeconds = expClaim.Value - currentUnixTime;
                    return TimeSpan.FromSeconds(remainingSeconds);
                }
            }
            catch (Exception e)
            {
            }

            return TimeSpan.Zero;
        }
    }
}
