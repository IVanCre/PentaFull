using SQLite;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaga_Client.Services.Repository
{
    public class ChatInfoEntity
    {
        [PrimaryKey, AutoIncrement]
        [Indexed(Name = "ID", Unique = true)]
        public int ID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public string[] Users { get; set; }

    }
}
