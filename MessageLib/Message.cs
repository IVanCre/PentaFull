
using System.Text.Json.Serialization;


namespace MessageLib
{
    //  ИМЕНА ПЕРЕМЕННЫХ В КОНСТРУКТОРЕ ДОЛЖНЫ БЫТЬ ИДЕНТИЧНЫ ИМЕНАМ ПОЛЕЙ !!!!!!!!!
    public class Message
    {
        public long ID { get; private set; }
        public int FromID { get; private set; }
        public int ChatID { get; private set; }
        public int ToID { get; private set; }
        public MessageType Type { get; private set; }
        public byte[] Data { get; private set; }

        [JsonConstructor]
        public Message(
           long ID,
           int FromID,
           int ChatID,
           int ToID,
           MessageType Type,
           byte[] Data)
        {
            this.ID = ID;
            this.FromID = FromID;
            this.ChatID = ChatID;
            this.ToID = ToID;
            this.Type = Type;
            this.Data = Data;
        }

        public static long GenerateIDByTime()//при высокой интенсивности, могут выскакивать повторы))
        {
            return (long)(DateTime.Now- DateTime.Parse("01.01.2025")).TotalMilliseconds;
        }
    }
}
