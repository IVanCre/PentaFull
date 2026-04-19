
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
        public DateTimeOffset UtcTimestamp { get; private set; }
        public bool IsSendedToServer { get; set; }

        [JsonConstructor]
        public Message(
           long ID,
           int FromID,
           int ChatID,
           int ToID,
           MessageType Type,
           byte[] Data,
           DateTimeOffset UtcTimestamp,
           bool IsSendedToServer)
        {
            this.ID = ID;
            this.FromID = FromID;
            this.ChatID = ChatID;
            this.ToID = ToID;
            this.Type = Type;
            this.Data = Data;
            this.UtcTimestamp = UtcTimestamp;
            this.IsSendedToServer = IsSendedToServer;
        }

        public void SetServerID(long newID)//используется сервером для переопределения значения, которое пришло от клиента
        {
            if(newID>0)//пеоложительные у сервера
                this.ID = newID;
        }

//при высокой интенсивности, могут выскакивать повторы))
// на сервере заменяются на собственные ID -т.к. на сервак может прийти 2 сообщения с одинаковым ID от 2 клиентов одновременно
        public static long GenerateLocalIDByTime()
        {
            return (long)(DateTime.Now- DateTime.Parse("01.01.2026")).TotalMilliseconds * -1;//*-1 позволит разграничивать локальные сообщения и от сервера в локальной БД
        }
    }
}
