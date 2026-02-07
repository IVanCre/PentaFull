using MessageLib;
using SQLite;


namespace Client.Platforms.Android.Repository
{
    internal class MessageItem
    {
        [PrimaryKey]
        public long ID { get; set; }

        [Indexed(Name ="IsSended")]
        public bool IsSended { get; set; }
        public int FromID { get; set; }

        [Indexed(Name = "ChatID")]
        public int ChatID { get; set; }
        public int ToID { get; set; }
        public MessageType Type { get; set; }
        public byte[] Data { get; set; }
    }
}
