using MessageLib;

namespace Message_Server.Interfaces
{
    public interface IMessageSaver
    {
        public void Save(List<Message> messages);
        public void Save(Message msg);
    }
}
