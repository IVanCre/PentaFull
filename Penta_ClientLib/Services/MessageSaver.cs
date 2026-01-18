using Penta_ClientLib.Interfaces;
using MessageLib;


namespace Penta_ClientLib.Services
{

    internal class MessageSaver: IMessageProcessor
    {
        private IMessageHolder _messHolder;
        private IWebClient _webClient;

        public event MessageRecieved RecievedMessage
        {
            add => _webClient.RecievedMessage += value;
            remove=>_webClient.RecievedMessage -= value;
        }


        public MessageSaver(
            IMessageHolder messProvider,
            IWebClient webClient)
        {
            _messHolder = messProvider;
            _webClient = webClient;
            RecievedMessage += Save;
        }
        private async void Save (Message msg)
        {
             await _messHolder.SaveMessage(msg);
        }


        public async Task<Tuple<bool, Exception>> SendMessage(Message message)
        {
            try
            {
                var saved= await _messHolder.SaveMessage(message);
                if (saved)
                {
                   var sended= await _webClient.SendMessage(message);
                   return Tuple.Create<bool,Exception>(sended, null);
                }
                else
                    return Tuple.Create(false, new Exception("Ошибка при сохранении сообщения в хранилище перед отправкой"));
            }
            catch (Exception ex)
            {
                return Tuple.Create(false,ex);
            }
        }
    }
}
