
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories;
using Penta_Server.Services.Repositories.Models;
using Penta_Server.StaticUtilits;
using Microsoft.EntityFrameworkCore;

namespace Penta_Server.Services
{
    public class TokenManager(
        IConfiguration config,
        ILogWriter logger) : ITokenManager
    {
        private IConfiguration _config = config;
        private ILogWriter _logger = logger;


        public string[] CreateTokenPack(int maskedID,string username, string pass)
        {
            string[] tokenPack = GenerateTokenPack(maskedID, username, pass);
            using (DB db= new DB(_config["WorkDB:ConnString"]))
            {
                string maskedPass = PasswordManager.Encrypt(pass, username);
                var finded =db.Users.FirstOrDefault(x => x.Name == username && x.MaskedPassword == maskedPass);
                db.Tokens.Where(x => x.User.ID ==finded.ID).ExecuteDelete();//удаляем старый пак

                db.Tokens.Add(
                    new TokenEntity() {
                        User= finded,
                        AccessToken= tokenPack[0],
                        RefreshToken = tokenPack[1]
                    });

                db.SaveChanges();
            }

            return tokenPack;
        }

        // На основе связки ник-пароль генерирует токен. Роль так же генерируется на основе связки
        private string[] GenerateTokenPack(int userID,string username, string pass)
        {
            string roleName = UserRoleCreator.GenerateRole(username, pass);
            var claims = new List<Claim> 
            { 
                new Claim("userID", userID.ToString()),//не используем имя, только maskedID
                new Claim(ClaimTypes.Role,roleName )//роли вшиваем в токен
            };

            return GenerateTokenPack(claims);
        }
        private string[] GenerateTokenPack(IEnumerable<Claim> claims)
        {
            var accessJwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(Convert.ToInt32(_config["Jwt:LifeTimeMinutes"]))),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                        SecurityAlgorithms.HmacSha256));

            var refreshJwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(Convert.ToInt32(365))),//ну типа если ты год не заходишь -ну сорямба((
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                        SecurityAlgorithms.HmacSha256));

            return new string[]{
                new JwtSecurityTokenHandler().WriteToken(accessJwt),
                new JwtSecurityTokenHandler().WriteToken(refreshJwt) };
        }




        public string[] RefreshJwtToken(string refreshToken)
        {
            string[] tokenPack = new string[2];
            using (DB db = new DB(_config["WorkDB:ConnString"]))
            {
                var finded = db.Tokens.FirstOrDefault(x => x.RefreshToken == refreshToken);
                if(finded!=null)
                {
                    var claims = GetClaims(finded.AccessToken);//берем старую инфу
                    var newPack = GenerateTokenPack(claims);
                    finded.AccessToken = newPack[0];
                    finded.RefreshToken = newPack[1];
                    tokenPack = newPack;

                    db.SaveChanges();
                }
            }
            return tokenPack;
        }

        public int FindUserByToken(string accessToken)
        {
            using (DB db = new DB(_config["WorkDB:ConnString"]))
            {
                var finded =db.Tokens.FirstOrDefault(x => x.AccessToken == accessToken);
                if (finded != null)
                {
                    var findedUser = db.Users.FirstOrDefault(x => x.ID == finded.ID);
                    if (findedUser != null)
                        return findedUser.ID;
                }
            }

            _logger?.SaveForDEBUG("Юзер, указанный в токене, не обнаружен в БД");
            return -1;
        }

        private TimeSpan GetTokenLifeTime(string token)
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
            catch (Exception e)
            {
                _logger?.SaveError($"Ошибка чтения даты jwtToken: {e.Message}");
            }

            return TimeSpan.Zero;
        }
        private IEnumerable<Claim> GetClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Payload.Claims;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
