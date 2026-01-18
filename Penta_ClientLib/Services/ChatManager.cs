using Penta_ClientLib.Interfaces;
using Penta_ClientLib.MethodResults;
using MessageLib;

namespace Penta_ClientLib.Services
{

    internal class ChatManager : IChatManager
    {
        private IGroupChatProvider _chatProvider;
        private IMessageProcessor _messProcessor;
        private IContactManager _contactManager;
        private ISettingsProvider _settings;

        public event ChatChanged CreatedNewChat;
        public event ChatChanged ChatDeleted;
        public event ChatUserListChanged UserAdded;
        public event ChatUserListChanged UserRemoved;
        public event NewMessageInChat MessageAddedToChat;

        public ChatManager(
            IGroupChatProvider chatProvider,
            IContactManager contactManager,
            ISettingsProvider settings,
            IMessageProcessor messProcessor)
        {
            _chatProvider = chatProvider;
            _messProcessor = messProcessor;
            _messProcessor.RecievedMessage += ProcessResponce;
            _contactManager = contactManager;
            _settings = settings;
        }

        private async void ProcessResponce(Message msg)//просматриваем ответы от сервера
        {
            switch(msg.Type)
            {
                case MessageType.EnterToGroupResponce:          await ProcessInviteUserToGroupResponce(msg); break;
                case MessageType.LeaveGroupResponce:            await ProcessUserLeaveGroupResponce(msg);break;
                case MessageType.RemoveUserFromGroupResponce:   await ProcessDeleteUserFromGroupResponce(msg); break;
                case MessageType.CreateGroupResponce:           await ProcessCreateGroupChatResponce(msg);break;
                case MessageType.DeleteGroupResponce:           await ProcessDeleteGroupResponce(msg);break;

                default:                                        await ProcessMessageFromUser(msg);break;
            }
        }

        public async Task<BOOLResult> SendCreateGroupChat(string chatName)//запрос
        {
            try
            {
                int currentUserID = _settings.GetValueByName<int>("userID");
                var msg = new Message(
                    -1,
                    currentUserID,
                    -1,
                    -1,
                    MessageType.CreateGroupRequest,
                    MessageUtils.TextToBytes(chatName));

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return new BOOLResult(false, e);
            }
        }
        private async Task ProcessCreateGroupChatResponce(Message msg)//ответ на запрос
        {
            if (msg.ChatID != -1)//значит сервак успешно создал
            {
                if(await _chatProvider.CreateChat(msg.GetDataLikeString(), msg.ChatID))
                    CreatedNewChat?.Invoke(msg.ChatID);
            }
        }


        public async Task<BOOLResult> SendDeleteGroupChat(string chatName)
        {
            try
            {
                int currentUserID = _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                var msg = new Message(
                    -1,
                    currentUserID,
                    chatID,
                    -1,
                    MessageType.DeleteGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch(Exception e)
            {
                return new BOOLResult(false, e);
            }
        }
        private async Task ProcessDeleteGroupResponce(Message msg)
        {
            if (msg.GetDataLikeBoolean())
            {
                if(await _chatProvider.DeleteChat(msg.ChatID))
                    ChatDeleted?.Invoke(msg.ChatID);
            }
        }


        public async Task<BOOLResult> SendDeleteUserFromGroupChat(string chatName, string userName)
        {
            try
            {
                int currentUserID = _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                int userID = await _contactManager.GetUserID(userName);
                var msg = new Message(
                    -1,
                    currentUserID,
                    chatID,
                    userID,
                    MessageType.RemoveUserFromGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return new BOOLResult(false, e);
            }
        }
        private async Task ProcessDeleteUserFromGroupResponce(Message msg)
        {
            if (await _chatProvider.RemoveUserFromChat(msg.GetDataLikeInt(), msg.ChatID))
                UserRemoved?.Invoke(msg.ChatID, msg.GetDataLikeInt());
        }


        public async Task<BOOLResult> SendLeaveGroupChat(string chatName)
        {
            try
            {
                int currentUserID = _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                var msg = new Message(
                    -1,
                    currentUserID,
                    chatID,
                    -1,
                    MessageType.LeaveGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return new BOOLResult(false, e);
            }
        }
        private async Task ProcessUserLeaveGroupResponce(Message msg)
        {
            if(await _chatProvider.RemoveUserFromChat(msg.GetDataLikeInt(), msg.ChatID))
                UserRemoved?.Invoke(msg.ChatID, msg.GetDataLikeInt());
        }


        public async Task<BOOLResult> SendInviteUserToGroupChat(string userName, string chatName)
        {
            try
            {
                int currentUserID = _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                int userID = await _contactManager.GetUserID(userName);
                var msg = new Message(
                    -1,
                    currentUserID,
                    chatID,
                    userID,
                    MessageType.EnterToGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return new BOOLResult(false, e);
            }
        }
        private async Task ProcessInviteUserToGroupResponce(Message msg)
        {
            await _contactManager.AutoAddNewUserContact(msg.GetDataLikeString(), msg.FromID);//сразу сохраняем новый контакт
            if (await _chatProvider.AddUserToChat(msg.FromID, msg.ChatID))
                UserAdded?.Invoke(msg.ChatID, msg.FromID);
        }



        public async Task<BOOLResult> AddMessageToChat(string chatName,string userName,MessageType type,byte[] data)
        {
            try
            {
                int chatID = await _chatProvider.GetChatID(chatName);
                int toUserID = await _contactManager.GetUserID(userName);
                var currUser = _settings.GetValueByName<int>("userID");
                return await _messProcessor.SendMessage(
                            new Message(
                                -1,
                                currUser,
                                chatID,
                                toUserID,
                                type,
                                data
                            ));
            }
            catch(Exception e)
            {
                return new BOOLResult(false, e);
            }
        }

        private async Task ProcessMessageFromUser(Message msg)
        {
            if(msg.ToID== _settings.GetValueByName<int>("userID"))//сообщение адресовано именно нам
            {

                if (await _contactManager.AutoAddNewUserContact(msg.FromID.ToString(), msg.FromID))//сохраняем id как имя(все что есть)
                {
                    await _chatProvider.CreateChat(msg.FromID.ToString(), msg.ChatID);//создаем новый чат
                    await _chatProvider.AddUserToChat(msg.FromID, msg.ChatID);// прикрепляем юзера к чату
                }

                if(msg.ChatID!=-1)//указан групповой чат
                    MessageAddedToChat?.Invoke(msg.ChatID, msg);
                else//обычный приватный чат
                {
                    int chatID =await _chatProvider.GetChatID(msg.FromID.ToString());
                    MessageAddedToChat?.Invoke(chatID, msg);
                }
            }
        }        
       
    }
}
