using MessageLib;
using SQLite;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaga_Client.Services.Repository
{
    public class MessageEntity
    {
        [PrimaryKey, AutoIncrement]
        [Indexed(Name = "ID", Unique = true)]
        public int ID { get; set; }

        public bool IsSended { get; set; }//была ли выполнена автодоставка при покдлючении(чтобы потом удалить это сообщение)
        public DateTime SavedTime { get; set; }//когда оно было сохранено в БД


        [StringLength(50)]
        [Indexed(Name = "ToUser_FromUser_Index", Order = 2, Unique = true)]//объявление составного индекса
        public string FromUser { get; set; }


        [StringLength(50)]
        [Indexed(Name= "ToUser_FromUser_Index", Order = 1, Unique = true)]
        public string ToUser { get; set; }


        [Indexed(Name="Type")]
        public MessageLib.DataType Type { get; set; }
        public byte[] MessageObject { get; set; }

    }
}
