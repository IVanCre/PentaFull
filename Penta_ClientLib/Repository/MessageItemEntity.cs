using MessageLib;
using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class MessageItemEntity
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [Indexed(Name ="IsSended")]
        public bool IsSended { get; set; }
        public int FromID { get; set; }

        [Indexed(Name = "ChatID")]
        public int ChatID { get; set; }
        public int ToID { get; set; }
        public MessageType Type { get; set; }
        public byte[] Data { get; set; }

        [Indexed(Name = "TimestampMilisec")]
        public long TimestampMilisec { get; set; }//т.к. не всегда корректно работает с чистым datetime
    }
}
