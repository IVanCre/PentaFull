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


        public async void Save(Message message,long sharedDataMarker, int dataCopyCount)
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

                                var sharedDataID = _messageRepo.SaveDataLikeShared(msgPack.SharedMarker, msgPack.Message.Data, dataCopyCount);
                                _messageRepo.SaveWithData(msgPack.Message, sharedDataID);
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
            long marker)
        {
            public Message Message { get; private set; } = msg;
            public int DataCopyCount { get; private set; } = count;
            public long SharedMarker { get; private set; } = marker;//какие сообщения имеют общие Data
        }

    }
}
