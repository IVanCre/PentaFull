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
        private ConcurrentQueue<MessagePack> _inputMessages = new();
        private bool taskWork = false;
        public event MesageSaved MessageSaved;//отдает сохраненное сообщение


        public async void Save(Message message,Guid sharedDataMarker, int dataCopyCount)
        {
            _inputMessages.Enqueue(new MessagePack(message,dataCopyCount, sharedDataMarker));

            if (_inputMessages.Count > 0 && !taskWork)
            {
                await Task.Factory.StartNew(() =>
                {
                    taskWork = true;
                    while (_inputMessages.Count > 0)
                    {
                        if (_inputMessages.TryDequeue(out MessagePack msgPack))
                        {
                            if (message.Data != null)
                            {
                                if (dataCopyCount < 1)//защита от дурака, т.к. этот метод всегда идет с данными
                                    dataCopyCount = 1;

                                _messageRepo.SaveWithData(
                                    msgPack.Message,
                                    msgPack.SharedMarker,
                                    dataCopyCount);
                            }
                            else
                                _messageRepo.SaveWithoutData(msgPack.Message);

                            MessageSaved?.Invoke(msgPack.Message);
                        }
                    }
                    taskWork = false;
                });
            }
        }

        private class MessagePack(
            Message msg,
            int count,
            Guid marker)
        {
            public Message Message { get; private set; } = msg;
            public int DataCopyCount { get; private set; } = count;
            public Guid SharedMarker { get; private set; } = marker;//какие сообщения имеют общие Data
        }

    }
}
