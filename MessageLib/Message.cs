
using System.Text.Json.Serialization;


namespace MessageLib
{
    //  ИМЕНА ПЕРЕМЕННЫХ В КОНСТРУКТОРЕ ДОЛЖНЫ БЫТЬ ИДЕНТИЧНЫ ИМЕНАМ ПОЛЕЙ !!!!!!!!!
    public class Message
    {
        public Guid ID { get; private set; }
        public int FromID { get; private set; }
        public int ChatID { get; private set; }
        public int ToID { get; private set; }
        public MessageType Type { get; private set; }
        public byte[] Data { get; private set; }
        public DateTimeOffset UtcTimestamp { get; private set; }
        public bool IsSendedToServer { get; set; }

        [JsonConstructor]
        public Message(
           Guid ID,
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

        /// <summary>
        /// Используется, когда сообщение успешно пересекает границу между сервером и клиентом.
        /// Чтобы не при вставке в разные БД не было конфликтов-повторов ID
        /// </summary>
        public void GenerateNewID()
        {
            this.ID = Guid.NewGuid();
        }
    }
}
