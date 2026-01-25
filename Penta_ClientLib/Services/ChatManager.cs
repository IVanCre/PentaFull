using Penta_ClientLib.Interfaces;
using MessageLib;
using Penta_ClientLib.DataStructs;

namespace Penta_ClientLib.Services
{
    internal class ChatManager : IChatManager
    {
        private IChatHolder _chatHolder;
        private IWebClient _messSender;
        private IContactConverter _contactConverter;
        private ISettingsHolder _settingsHolder;
        private IMessageHolder _messHolder;

        public ChatManager(
            IChatHolder chatProvider,
            IContactConverter contactConverter,
            ISettingsHolder settings,
            IWebClient messSender,
            IMessageHolder messHolder)
        {
            _chatHolder = chatProvider;
            _messSender = messSender;
            _contactConverter = contactConverter;
            _settingsHolder = settings;
            _messHolder = messHolder;
        }

        public async Task<Tuple<bool,Exception>> SendCreateGroupChat(string chatName)//запрос
        {
            try
            {
                int currentUserID = await _settingsHolder.GetValueByName<int>("userID");
                var msg = MessageFactory.CreateGroupChat_Request(currentUserID, chatName); 
                await _messHolder.SaveMessage(msg);

                var sended= await _messSender.SendMessage(msg);
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
                int currentUserID = await _settingsHolder.GetValueByName<int>("userID");
                var msg = MessageFactory.InviteUserToGroupChat_UserResponse(currentUserID, chatID, acceptInvite);
                await _messHolder.SaveMessage(msg);

                var sended = await _messSender.SendMessage(msg);
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
                int currentUserID =await _settingsHolder.GetValueByName<int>("userID");
                int userID = _contactConverter.ExtractUserID(userConnectID);
                var msg = MessageFactory.InviteUserToGroupChat_Request(currentUserID,chatID,userID);
                await _messHolder.SaveMessage(msg);

                var sended = await _messSender.SendMessage(msg);
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
                int currentUserID =await _settingsHolder.GetValueByName<int>("userID");
                int userID =_contactConverter.ExtractUserID(userConnectID);
                var msg = MessageFactory.DeleteUserFromGroupChat_Request(currentUserID, userID, chatID);
                await _messHolder.SaveMessage(msg);

                var sended = await _messSender.SendMessage(msg);
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
                int currentUserID = await _settingsHolder.GetValueByName<int>("userID");
                var msg = MessageFactory.DeleteUserFromGroupChat_Request(currentUserID, currentUserID, chatID);
                await _messHolder.SaveMessage(msg);

                var sended = await _messSender.SendMessage(msg);
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
                int currentUserID =await _settingsHolder.GetValueByName<int>("userID");
                var msg =MessageFactory.DeleteGroupChat_Request(currentUserID, chatID);
                await _messHolder.SaveMessage(msg);

                var sended = await _messSender.SendMessage(msg);
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
                int currentUserID = await _settingsHolder.GetValueByName<int>("userID");
                Message msg;
                if (chatID == -1)//значит это НЕ групповой чат
                    msg = MessageFactory.UserToUser(currentUserID, recieverID, type, data);
                else
                    msg = MessageFactory.UserToGroupChat(currentUserID, chatID, type, data);
                await _messHolder.SaveMessage(msg);

                var sended = await _messSender.SendMessage(msg);
                return Tuple.Create<bool, Exception>(sended, null);
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }


        public async Task<Tuple<List<ChatInfo>, Exception>> GetAllChatsInfo()
        {
            var list =await _chatHolder.GetAllChats();
            return Tuple.Create<List<ChatInfo>,Exception>(list, null);
        }
    }
}
