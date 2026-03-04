using Penta_ClientLib.Interfaces;
using MessageLib;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Services;

namespace Penta_ClientLib.Services
{
    internal class ChatManager : IChatManager
    {
        private IChatHolder _chatHolder;
        private IWebClient _webClient;
        private ISettingsProvider _settingsHolder;
        private IMessageHolder _messHolder;

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
                var currUserID = await _settingsHolder.GetUserID();
                var msg = MessageFactory.CreateGroupChat_Request(currUserID, chatName); 
                await _messHolder.SaveMessage(msg);

                var sended= await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }


        public async Task<Tuple<bool,Exception>> SendResponseToInvite(int chatID, bool acceptInvite)
        {
            try
            {
                var currUserID = await _settingsHolder.GetUserID();
                var msg = MessageFactory.InviteUserToGroupChat_UserResponse(currUserID, chatID, acceptInvite);
                await _messHolder.SaveMessage(msg);

                var sended = await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }
        public async Task<Tuple<bool,Exception>> SendInviteUserToGroupChat(int chatID, string userConnectID)
        {
            try
            {
                var currUserID = await _settingsHolder.GetUserID();
                int userID = ContactConverter.ExtractUserID(userConnectID);
                var msg = MessageFactory.AddUserToGroupChat_Request(currUserID, chatID,userID);
                await _messHolder.SaveMessage(msg);

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
                var currUserID = await _settingsHolder.GetUserID();
                int userID =ContactConverter.ExtractUserID(userConnectID);
                var msg = MessageFactory.DeleteUserFromGroupChat_Request(currUserID, userID, chatID);
                await _messHolder.SaveMessage(msg);

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
                var currUserID = await _settingsHolder.GetUserID();
                var msg = MessageFactory.DeleteUserFromGroupChat_Request(currUserID, currUserID, chatID);
                await _messHolder.SaveMessage(msg);

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
                var currUserID = await _settingsHolder.GetUserID();
                var msg =MessageFactory.DeleteGroupChat_Request(currUserID, chatID);
                await _messHolder.SaveMessage(msg);

                var sended = await _webClient.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch(Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }


        public async Task<Tuple<bool, Exception>> AddMessageToChat(int chatID, int recieverID, MessageType type, byte[] data)
        {
            try
            {
                Message msg;
                var currUserID = await _settingsHolder.GetUserID();
                if (chatID < 0)//значит это приватный чат
                {
                    msg = MessageFactory.UserToUser(currUserID, recieverID, type, data);
                    await _messHolder.SaveMessage(//сохраняем копию, у которой указа локальный идентификатор чата(чтоб знать от какого чата это сообщение)
                        new Message(
                            msg.ID,
                            msg.FromID,
                            chatID,
                            msg.ToID,
                            msg.Type,
                            msg.Data,
                            msg.UtcTimestamp));
                }
                else
                {
                    msg = MessageFactory.UserToGroupChat(currUserID, chatID, type, data);
                    await _messHolder.SaveMessage(msg);
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
        public Task<int> CreatePrivateChat(string chatName) => _chatHolder.CreatePrivateChat(chatName);
        public Task<bool> DeletePrivateChat(int chatID) => _chatHolder.DeleteChat(chatID);
    }
}
