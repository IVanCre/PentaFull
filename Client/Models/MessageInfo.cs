namespace Client.Models
{
    public enum Direction
    {
        Input,
        Output
    }
    public class MessageInfo
    {
        public Direction Type { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset TimestampData { get; set; }
        public string Timestamp 
        { 
            get
            {
                if (TimestampData != default(DateTimeOffset))
                    return TimestampData.ToLocalTime().ToString("dd.MM.yy HH:mm:ss");//отображение в нашем часовом поясе
                else
                    return "??:??:??";
            }
        }
    }
}