using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Models
{
    public enum Direction
    {
        Input,
        Output
    }
    public class MessageInfo : INotifyPropertyChanged
    {
        public long ID { get; set; }
        public Direction Type { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset TimestampData { get; set; }


        private bool _sended=false;
        public bool IsSended 
        {
            get => _sended;
            set
            {
                if (_sended != value)
                {
                    _sended = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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