using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Penta_Server.Services.Repositories.Models
{
    [Index("ID", IsUnique =true)]
    [Index("AccessHash")]
    [Index("RefreshHash")]
    public class TokenEntity
    {
        public int ID {  get; set; }
        public UserEntity User{ get; set; }

        [MaxLength(70)]
        public string AccessHash { get; set; }//отказался от фактических данных в пользу хэшей
        [MaxLength(70)]
        public string RefreshHash { get; set; }
    }
}
