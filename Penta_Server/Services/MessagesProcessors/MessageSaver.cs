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

        public async void Save(Message message, int copyCount)
        {
            _inputMessages.Enqueue(message);

            if (_inputMessages.Count > 0 && !taskWork)
            {
                await Task.Factory.StartNew(async() =>
                {
                    taskWork = true;

                    if(copyCount<1)//защита от дурака
                        copyCount = 1;

                    while (_inputMessages.Count > 0)
                    {
                        if (_inputMessages.TryDequeue(out Message msg))
                        {
                            if (message.Data != null)
                            {
                                var sharedDataID = await _messageRepo.SaveDataLikeShared(msg.Data, copyCount);
                                _messageRepo.SaveWithData(msg, sharedDataID);
                            }
                            else
                                _messageRepo.SaveWithoutData(msg);

                            MessageSaved?.Invoke(msg);
                        }
                    }
                    taskWork = false;
                });
            }
        }
    }
}
