using Penta_ClientLib.Interfaces;
using MessageLib;
using Penta_ClientLib.DataStructs;


namespace Penta_ClientLib.Services
{
    internal class ChatManager : IChatManager
    {
        private IChatHolder _chatHolder;
        private IWebClient _webClient;
        private ISettingsProvider _settingsHolder;
        private IMessageHolder _messHolder;
        private int? _currentUserID;

        public ChatManager(
            IChatHolder chatProvider,
            ISettingsProvider settings,
            IWebClient client,
            IMessageHolder messHolder)
        {
            _chatHolder = chatProvider;
            _webClient = client;
            _settingsHolder = settings;
            _messHolder = messHolder;
        }

        public async Task<Tuple<bool,Exception>> SendCreateGroupChat(string chatName)//запрос
        {
            try
            {
                if(_currentUserID==null)
                    _currentUserID = await _settingsHolder.GetUserID();

                var msg = MessageFactory.CreateGroupChat_Request(_currentUserID.Value, chatName); 
                await _messHolder.SaveMessage(msg,false);

                var sended= await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }


        public async Task<Tuple<bool,Exception>> SendAddUserToGroupChat(int chatID, string userConnectIDForAdd)
        {
            try
            {
                if (_currentUserID == null)
                    _currentUserID = await _settingsHolder.GetUserID();

                int userID = ContactConverter.ExtractUserID(userConnectIDForAdd);
                var msg = MessageFactory.CreateAddUserToGroupChat_Request(_currentUserID.Value, chatID,userID);
                await _messHolder.SaveMessage(msg,false);

                var sended = await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }


        public async Task<Tuple<bool,Exception>> SendDeleteUserFromGroupChat(int chatID, string userConnectID)
        {
            try
            {
                if (_currentUserID == null)
                    _currentUserID = await _settingsHolder.GetUserID();

                int userID =ContactConverter.ExtractUserID(userConnectID);
                var msg = MessageFactory.CreateDeleteUserFromGroupChat_Request(_currentUserID.Value, userID, chatID);
                await _messHolder.SaveMessage(msg,false);

                var sended = await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }
        public async Task<Tuple<bool, Exception>> SendLeaveGroupChat(int chatID)//сам себя удаляем
        {

            try
            {
                if (_currentUserID == null)
                    _currentUserID = await _settingsHolder.GetUserID();

                var msg = MessageFactory.CreateDeleteUserFromGroupChat_Request(_currentUserID.Value, _currentUserID.Value, chatID);
                await _messHolder.SaveMessage(msg,false);

                var sended = await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }


        public async Task<Tuple<bool,Exception>> SendDeleteGroupChat(int chatID)
        {
            try
            {
                if (_currentUserID == null)
                    _currentUserID = await _settingsHolder.GetUserID();

                var msg =MessageFactory.CreateDeleteGroupChat_Request(_currentUserID.Value, chatID);
                await _messHolder.SaveMessage(msg,false);

                var sended = await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch(Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }


        public async Task<Tuple<bool, Exception>> AddMessageToChat(int chatID, int recieverID, MessageType type, byte[] data, long? messageID)
        {
            try
            {
                Message msg;
                if (_currentUserID == null)
                    _currentUserID = await _settingsHolder.GetUserID();

                if (chatID < 0)//значит это приватный чат
                {
                    msg = MessageFactory.CreateUserToUser(_currentUserID.Value, recieverID, type, data, messageID);
                    await _messHolder.SaveMessage(//сохраняем копию, у которой указа локальный идентификатор чата(чтоб знать от какого чата это сообщение)
                        new Message(
                            msg.ID,
                            msg.FromID,
                            chatID,
                            msg.ToID,
                            msg.Type,
                            msg.Data,
                            msg.UtcTimestamp,
                            false),
                            false);
                }
                else
                {
                    msg = MessageFactory.CreateUserToGroupChat(_currentUserID.Value, chatID, type, data, messageID);
                    await _messHolder.SaveMessage(msg,false);
                }

                var sended = await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }


        public Task<List<ChatInfo>> GetAllChatsInfo() => _chatHolder.GetAllChats();
        public Task<ChatInfo> GetChatByID(int chatID)=> _chatHolder.GetChatByID(chatID);
        public Task<int> CreatePrivateChat(string chatName) => _chatHolder.CreatePrivateChat(chatName);
        public Task<bool> DeletePrivateChat(int chatID) => _chatHolder.DeleteChat(chatID);

        public Task<bool> AmCreatedThisGroupChat(int chatID)=>_chatHolder.AmCreatedThisGroupChat(chatID);

    }
}
