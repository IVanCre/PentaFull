using Penta_ClientLib.Interfaces;
using MessageLib;

namespace Penta_ClientLib.Services
{

    internal class ChatManager : IChatManager
    {
        private IChatHolder _chatProvider;
        private IMessageProcessor _messProcessor;
        private IMessageHolder _messHolder;
        private IContactManager _contactManager;
        private ISettingsHolder _settings;

        public event ChatChanged CreatedNewChat;
        public event ChatChanged ChatDeleted;
        public event ChatUserListChanged UserAdded;
        public event ChatUserListChanged UserRemoved;
        public event NewMessageInChat MessageAddedToChat;

        public ChatManager(
            IChatHolder chatProvider,
            IContactManager contactManager,
            ISettingsHolder settings,
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

        public async Task<Tuple<bool,Exception>> SendCreateGroupChat(string chatName)//запрос
        {
            try
            {
                int currentUserID = await _settings.GetValueByName<int>("userID");
                var msg = new Message(
                    Message.GenerateIDByTime(),
                    currentUserID,
                    -1,
                    -1,
                    MessageType.CreateGroupRequest,
                    MessageUtils.TextToBytes(chatName));

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
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


        public async Task<Tuple<bool,Exception>> SendDeleteGroupChat(string chatName)
        {
            try
            {
                int currentUserID =await _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                var msg = new Message(
                    Message.GenerateIDByTime(),
                    currentUserID,
                    chatID,
                    -1,
                    MessageType.DeleteGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch(Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }
        private async Task ProcessDeleteGroupResponce(Message msg)
        {
            if (msg.GetDataLikeBoolean())
            {
                if (await _chatProvider.DeleteChat(msg.ChatID))
                {
                    await _messHolder.DeleteByChatID(msg.ChatID);
                    ChatDeleted?.Invoke(msg.ChatID);
                }
            }
        }


        public async Task<Tuple<bool,Exception>> SendDeleteUserFromGroupChat(string chatName, string userName)
        {
            try
            {
                int currentUserID =await _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                int userID = await _contactManager.GetUserID(userName);
                var msg = new Message(
                    Message.GenerateIDByTime(),
                    currentUserID,
                    chatID,
                    userID,
                    MessageType.RemoveUserFromGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }
        private async Task ProcessDeleteUserFromGroupResponce(Message msg)
        {
            if (await _chatProvider.RemoveUserFromChat(msg.GetDataLikeInt(), msg.ChatID))
                UserRemoved?.Invoke(msg.ChatID, msg.GetDataLikeInt());
        }


        public async Task<Tuple<bool,Exception>> SendLeaveGroupChat(string chatName)
        {
            try
            {
                int currentUserID =await _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                var msg = new Message(
                    Message.GenerateIDByTime(),
                    currentUserID,
                    chatID,
                    -1,
                    MessageType.LeaveGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }
        private async Task ProcessUserLeaveGroupResponce(Message msg)
        {
            if(await _chatProvider.RemoveUserFromChat(msg.GetDataLikeInt(), msg.ChatID))
                UserRemoved?.Invoke(msg.ChatID, msg.GetDataLikeInt());
        }


        public async Task<Tuple<bool,Exception>> SendInviteUserToGroupChat(string userName, string chatName)
        {
            try
            {
                int currentUserID =await _settings.GetValueByName<int>("userID");
                int chatID = await _chatProvider.GetChatID(chatName);
                int userID = await _contactManager.GetUserID(userName);
                var msg = new Message(
                    Message.GenerateIDByTime(),
                    currentUserID,
                    chatID,
                    userID,
                    MessageType.EnterToGroupRequest,
                    null);

                return await _messProcessor.SendMessage(msg);
            }
            catch (Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }
        private async Task ProcessInviteUserToGroupResponce(Message msg)
        {
            await _contactManager.AutoAddNewUserContact(msg.GetDataLikeString(), msg.FromID);//сразу сохраняем новый контакт
            if (await _chatProvider.AddUserToChat(msg.FromID, msg.ChatID))
                UserAdded?.Invoke(msg.ChatID, msg.FromID);
        }



        public async Task<Tuple<bool,Exception>> AddMessageToChat(string chatName,string userName,MessageType type,byte[] data)
        {
            try
            {
                int chatID = await _chatProvider.GetChatID(chatName);
                int toUserID = await _contactManager.GetUserID(userName);
                var currUser = await _settings.GetValueByName<int>("userID");
                return await _messProcessor.SendMessage(
                            new Message(
                                Message.GenerateIDByTime(),
                                currUser,
                                chatID,
                                toUserID,
                                type,
                                data
                            ));
            }
            catch(Exception e)
            {
                return Tuple.Create<bool, Exception>(false, e);
            }
        }

        private async Task ProcessMessageFromUser(Message msg)
        {
            if(msg.ToID== await _settings.GetValueByName<int>("userID"))//сообщение адресовано именно нам
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
