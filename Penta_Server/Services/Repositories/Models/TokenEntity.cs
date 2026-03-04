using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Penta_Server.Services.Repositories.Models
{
    [Index("ID", IsUnique =true)]
    [Index("AccessToken")]//т.к. часто по токену ищем юзера
    [Index("RefreshToken")]//т.к. часто по токену ищем юзера
    public class TokenEntity
    {
        public int ID {  get; set; }
        public UserEntity User{ get; set; }

        [MaxLength(1024)]
        public string AccessToken { get; set; }
        [MaxLength(1024)]
        public string RefreshToken { get; set; }
    }
}
