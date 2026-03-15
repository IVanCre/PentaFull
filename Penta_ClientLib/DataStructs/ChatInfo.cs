namespace Penta_ClientLib.DataStructs
{
    internal static class ImageResourceProvider
    {
        public static string PrivateChat = "private_chat.png";
        public static string GroupChat = "group_chat.png";
        public static string NewMessage = "message.png";
        public static string NotifyChat = "notify_chat.png";
    }

    public sealed class ChatInfo(
        int id,
        string name,
        ChatType type,
        bool haveUnreaded)
    {
        public  int ID { get; private set; } =id;
        public string ChatName { get; private set; } = name;

        public ChatType ChatType { get; private set; }= type;
        public string ImageSourceName//имя картинки, которую надо использовать для чата конкретного типа
        {
            get
            {
                switch(ChatType)
                {
                    case ChatType.Private: return ImageResourceProvider.PrivateChat;
                    case ChatType.Group: return ImageResourceProvider.GroupChat;
                    case ChatType.ReadOnly: return ImageResourceProvider.NotifyChat;
                }
                return string.Empty;
            }
        }
  
        public bool HaveUnreadedMessages { get; set; }=haveUnreaded;
        public string NotifySourceName//имя картинки, которую надо использовать для отображения новоого сообщения
        {
            get
            {
                if(HaveUnreadedMessages)
                    return ImageResourceProvider.NewMessage;
                else
                    return string.Empty;
            }
        }
    }
}
