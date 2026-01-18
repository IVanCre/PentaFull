using Penta_ClientLib.Interfaces;
using Penta_ClientLib.MethodResults;
using MessageLib;


namespace Penta_ClientLib.Services
{

    internal class MessageSaver: IMessageProcessor
    {
        private IMessageProvider _messProvider;
        private IWebClient _webClient;

        public event MessageRecieved RecievedMessage
        {
            add => _webClient.RecievedMessage += value;
            remove=>_webClient.RecievedMessage -= value;
        }


        public MessageSaver(
            IMessageProvider messProvider,
            IWebClient webClient)
        {
            _messProvider = messProvider;
            _webClient = webClient;
            RecievedMessage += Save;
        }
        private async void Save (Message msg)
        {
             await _messProvider.SaveMessage(msg);
        }


        public async Task<BOOLResult> SendMessage(Message message)
        {
            try
            {
                var saved= await _messProvider.SaveMessage(message);
                if (saved)
                {
                   var sended= await _webClient.SendMessage(message);
                   return new BOOLResult(sended, null);
                }
                else
                    return new BOOLResult(false, new Exception("Ошибка при сохранении сообщения в хранилище перед отправкой"));
            }
            catch (Exception ex)
            {
                return new BOOLResult(false,ex);
            }
        }
    }
}
