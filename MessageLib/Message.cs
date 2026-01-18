
using System.Text.Json.Serialization;


namespace MessageLib
{

    public class Message
    {
        public long ID { get; private set; }//на случай, если потребуется самим назначать
        public int FromID { get; private set; }
        public int ChatID { get; private set; }
        public int ToID { get; private set; }
        public MessageType Type { get; private set; }
        public byte[] Data { get; private set; }

        [JsonConstructor]
        public Message(
           long ID,
           int FromUserID,
           int GroupID,
           int ToUserID,
           MessageType Type,
           byte[] Data)
        {
            this.ID = ID;
            this.FromID = FromUserID;
            this.ChatID = GroupID;
            this.ToID = ToUserID;
            this.Type = Type;
            this.Data = Data;
        }

        public static long GenerateIDByTime()//при высокой интенсивности, могут выскакивать повторы))
        {
            return (long)(DateTime.Parse("01.01.2020") - DateTime.Now).TotalMilliseconds;
        }
    }
}
