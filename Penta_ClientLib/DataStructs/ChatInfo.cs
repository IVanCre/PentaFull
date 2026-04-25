using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        bool haveUnreaded) : INotifyPropertyChanged
    {
        public  int ID { get; private set; } =id;

        private string _chatName = name;
        public string ChatName
        {
            get => _chatName;
            set
            {
                if (_chatName != value)
                {
                    _chatName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ChatType ChatType { get; private set; }= type;

        private bool _haveUnreaded = haveUnreaded;
        public bool HaveUnreadedMessages
        {
            get => _haveUnreaded;
            set
            {
                if (_haveUnreaded != value)
                {
                    _haveUnreaded = value;
                    OnPropertyChanged();
                }
            }
        }


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

        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    }
}
