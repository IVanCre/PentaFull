using MessageLib;
using Microsoft.EntityFrameworkCore;



namespace Penta_Server.Services.Repositories.Models
{
    [Index("ID",IsUnique=true)]
    [Index("ToUserID","IsSended")]
    [Index("Type")]
    public class MessageEntity
    {
        public long ID { get; set; }

        public bool IsSended { get; set; }//была ли выполнена автодоставка при покдлючении(чтобы потом удалить это сообщение)
        public int FromUserID { get; set; }
        public int GroupID { get; set; }
        public int ToUserID { get; set; }

        public MessageType Type { get; set; }//чтобы EF в бибилотеку исходного класа не тащить
        public byte[] Data { get; set; }//по факту тут всегда byte[]
        public DateTime UtcTimestamp { get; set; }

    }
}
