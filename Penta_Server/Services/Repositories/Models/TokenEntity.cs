using Microsoft.EntityFrameworkCore;

namespace Penta_Server.Services.Repositories.Models
{
    [Index("ID", IsUnique =true)]
    [Index("Token")]//т.к. часто по токену ищем юзера
    public class TokenEntity
    {
        public int ID {  get; set; }
        public UserEntity User{ get; set; }
        public string Token { get; set; }
    }
}
