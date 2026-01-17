using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Message_Server.Services.Repositories.Models
{
    [Index("ID", IsUnique=true)]

    [Index("Name", "MaskedPassword")]
    public class UserEntity
    {
        public int ID { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        public string MaskedPassword { get; set; }//хранится в зашифрованном виде
    }
}
