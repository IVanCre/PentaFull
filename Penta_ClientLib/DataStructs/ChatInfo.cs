namespace Penta_ClientLib.DataStructs
{
    public sealed class ChatInfo(
        int id,
        string name,
        bool isGroupChat)
    {
        public  int ID { get; private set; } =id;
        public string ChatName { get; private set; } = name;

        public bool IsGroupChat { get; private set; }= isGroupChat;
        public string ImageSourceName
        {
            get
            {
                if (IsGroupChat)
                    return "group_chat.png";
                else
                    return "private_chat.png";
            }
        }
  
        public bool HaveUnreadedMessages { get; set; }
        public string NotifySourceName
        {
            get
            {
                if(HaveUnreadedMessages)
                    return "new_mesages.png";
                else
                    return string.Empty;
            }
        }
    }
}
