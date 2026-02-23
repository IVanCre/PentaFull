using Penta_Server.Interfaces;
using MessageLib;
using System.Collections.Concurrent;

namespace Penta_Server.Services.MessagesProcessors
{
    /// <summary>
    /// Сохраняет все входящие сообщения
    /// </summary>
    /// <param name="repository"></param>
    public class MessageSaver(IMessageRepository repository) : IMessageSaver
    {
        private readonly IMessageRepository _messageRepo = repository;
        private ConcurrentQueue<Message> _inputMessages = new ();
        private bool taskWork = false;
        public event MesageSaved MessageSaved;//отдает сохраненное сообщение


        public async void Save(List<Message> messages)
        {
            foreach (Message msg in messages)
                _inputMessages.Enqueue(msg);

            if (_inputMessages.Count > 0 && !taskWork)
            {
                await Task.Factory.StartNew(() =>
                {
                    taskWork = true;
                    while (_inputMessages.Count > 0)
                    {
                        if (_inputMessages.TryDequeue(out Message msg))
                        {
                            _messageRepo.Add(msg);
                            MessageSaved?.Invoke(msg);
                        }
                    }
                    taskWork = false;
                });
            }
        }

        public async void Save(Message message)
        {
            _inputMessages.Enqueue(message);

            if (_inputMessages.Count > 0 && !taskWork)
            {
                await Task.Factory.StartNew(() =>
                {
                    taskWork = true;
                    while (_inputMessages.Count > 0)
                    {
                        if (_inputMessages.TryDequeue(out Message msg))
                        {
                            _messageRepo.Add(msg);
                            MessageSaved?.Invoke(msg);
                        }
                    }
                    taskWork = false;
                });
            }
        }

    }
}
