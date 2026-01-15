using MessageLib;
using Microsoft.EntityFrameworkCore;



namespace Message_Server.Services.Repositories.Models
{
    [Index("ID",IsUnique=true)]
    [Index("ToUserID","IsSended")]
    [Index("Type")]
    public class MessageEntity
    {
        public int ID { get; set; }

        public bool IsSended { get; set; }//была ли выполнена автодоставка при покдлючении(чтобы потом удалить это сообщение)
        public int FromUserID { get; set; }
        public int GroupID { get; set; }
        public int ToUserID { get; set; }

        public MessageType Type { get; set; }
        public byte[] Data { get; set; }//по факту тут всегда byte[]

    }
}
