
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Message_Server.Interfaces;
using Message_Server.Services.Repositories;
using Message_Server.Services.Repositories.Models;
using Message_Server.StaticUtilits;

namespace Message_Server.Services
{
    public class TokenManager(
        IConfiguration config,
        ILogWriter logger) : ITokenManager
    {
        private IConfiguration _config = config;
        private ILogWriter _logger = logger;


        public string CreateToken(int maskedID,string username, string pass)
        {
            var token = GenerateToken(maskedID, username, pass);
            using (DB db= new DB(config["WorkDB:ConnString"]))
            {
                string maskedPass = PasswordManager.Encrypt(pass, username);
                var finded =db.Users.FirstOrDefault(x => x.Name == username && x.MaskedPassword == maskedPass);
                db.Tokens.Add(new TokenEntity() {User= finded,Token= token });
                db.SaveChanges();
            }

            return token;
        }

        public string GetToken(string username, string pass)
        {
            string token= string.Empty;
            using (DB db= new DB(config["WorkDB:ConnString"]))
            {
                string maskedPass = PasswordManager.Encrypt(pass, username);
                var findedUser = db.Users.FirstOrDefault(x => x.Name == username && x.MaskedPassword == maskedPass);
                if(findedUser!=null)
                {
                    var finded = db.Tokens.FirstOrDefault(x => x.User.ID== findedUser.ID);
                    if (TokenLifeTime(finded.Token).TotalMinutes > 5)
                        token = finded.Token;
                    else//почти просрочен, надо делать новый
                    {
                        finded.Token = GenerateToken(findedUser.ID,username, pass);
                        token = finded.Token;
                        db.SaveChanges();
                    }
                }
            }
            return token;
        }

        // На основе связки ник-пароль генерирует токен. Роль так же генерируется на основе связки
        private string GenerateToken(int userID,string username, string pass)
        {
            string roleName = UserRoleCreator.GenerateRole(username, pass);
            var claims = new List<Claim> 
            { 
                new Claim("userID", userID.ToString()),//не используем имя, только maskedID
                new Claim(ClaimTypes.Role,roleName )//роли вшиваем в токен
            };

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(Convert.ToInt32(_config["Jwt:LifeTimeMinutes"]))),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])), SecurityAlgorithms.HmacSha256));


            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }





        public string CreateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public int FindUserByToken(string token)
        {
            using (DB db = new DB(config["WorkDB:ConnString"]))
            {
                var finded =db.Tokens.FirstOrDefault(x => x.Token == token);
                if (finded != null)
                {
                    var findedUser = db.Users.FirstOrDefault(x => x.ID == finded.ID);
                    if (findedUser != null)
                        return findedUser.ID;
                }
            }

            _logger?.SaveSystemInfo("Юзер, указанный в токене, не обнаружен в БД");
            return -1;
        }

        private TimeSpan TokenLifeTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
                var expClaim = jwtToken.Payload.Exp;
                if (expClaim.HasValue)
                {
                    var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var remainingSeconds = expClaim.Value - currentUnixTime;
                    return TimeSpan.FromSeconds(remainingSeconds);
                }
            }
            catch (Exception)
            {

            }

            return TimeSpan.Zero;
        }
    }
}
