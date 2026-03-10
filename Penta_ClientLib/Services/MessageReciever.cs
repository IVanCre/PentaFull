using Penta_ClientLib.Interfaces;
using MessageLib;

namespace Penta_ClientLib.Services
{
    internal class MessageReciever: IMessageReciever
    {
        private IChatHolder _chatHolder;
        private IWebClient _messListener;
        private IMessageHolder _messHolder;


        public event ChatChanged CreatedNewChat;
        public event ChatChanged ChatDeleted;
        public event ChatUserListChanged UserAdded;//пока не используем
        public event ChatUserListChanged UserRemoved;//пока не используем
        public event NewMessageInChat MessageAddedToChat;
        public event AccountDeleted AccountDeleted;


        public MessageReciever(
            IWebClient messListener,
            IChatHolder chatProvider,
            IMessageHolder messHolder
            )
        {
            _messListener = messListener;
            _messListener.RecievedMessage += ProcessResponce;
            _chatHolder = chatProvider;
            _messHolder = messHolder;
        }        
        
        private async void ProcessResponce(Message msg)//просматриваем ответы от сервера
        {
            switch (msg.Type)
            {
                //сохранение системных сообщений не требуется
                case MessageType.AddUserToGroupResponse:        AddUserToGroupResponce(msg); break;
                case MessageType.UserAddedToGroupResponse:      await UserAddedToGroupChat(msg);break;
                case MessageType.RemoveUserFromGroupResponce:   DeleteUserFromGroupResponce(msg); break;
                case MessageType.CreateGroupResponce:           await CreateGroupChatResponce(msg);break;
                case MessageType.DeleteGroupResponce:           await DeleteGroupResponce(msg);break;
                case MessageType.DeleteSelfAccountResponce:     AccountDeleted?.Invoke(); ; break;

                //указанные сообщения сохранятся
                case MessageType.Text:
                case MessageType.Picture:
                case MessageType.Voice:                         await ProcessMessageFromUser(msg);break;
            }
        }


        private async Task UserAddedToGroupChat(Message msg)
        {
            if (msg.ChatID != -1)//значит сервак успешно создал
            {
                var chatName = msg.GetDataLikeString();
                if (await _chatHolder.CreateGroupChat(msg.ChatID, chatName, false))
                    CreatedNewChat?.Invoke(msg.ChatID, chatName);

                AddUserToGroupResponce(msg);
            }
        }

        private async Task CreateGroupChatResponce(Message msg)//ответ на наш запрос(значит мы являемся админом)
        {
            if (msg.ChatID!=-1)//значит сервак успешно создал
            {
                var chatName = msg.GetDataLikeString();
                if(await _chatHolder.CreateGroupChat(msg.ChatID,chatName, true ))
                    CreatedNewChat?.Invoke(msg.ChatID,chatName);

                AddUserToGroupResponce(msg);
            }
        }

        private void AddUserToGroupResponce(Message msg)
        {
            UserAdded?.Invoke(msg.ChatID, msg.ToID);
        }
        private void DeleteUserFromGroupResponce(Message msg)
        {
            var userIDToDelete = msg.GetDataLikeInt();
            UserRemoved?.Invoke(msg.ChatID, userIDToDelete);
        }        
        
        private async Task DeleteGroupResponce(Message msg)
        {
            if (await _chatHolder.DeleteChat(msg.ChatID))
                ChatDeleted?.Invoke(msg.ChatID,"");
        }


        private async Task ProcessMessageFromUser(Message msg)
        {
            if (msg.ChatID == -1)//личное сообщение
            {
                var userConnectionID = ContactConverter.ConvertUserIDToContactID(msg.FromID);
                var chatID = await _chatHolder.GetChatID(userConnectionID);//чаты ВСЕГДА хранятся с именем в виде contactID
                if (chatID != 0)//чат с указанным connectID есть
                {
                    if (await _messHolder.SaveMessage(
                        new Message(
                            msg.ID,
                            msg.FromID,
                            chatID,//сохраняем с указанным  идентификатором чата
                            msg.ToID,
                            msg.Type,
                            msg.Data,
                            msg.UtcTimestamp),
                        true))
                        MessageAddedToChat?.Invoke(chatID, msg);
                }
                else//нет чата с указанным connectID
                {
                    chatID = await _chatHolder.CreatePrivateChat(userConnectionID);
                    if (chatID != 0)//значит чат создан
                    {
                        CreatedNewChat?.Invoke(chatID, userConnectionID);

                        if (await _messHolder.SaveMessage(
                            new Message(
                                msg.ID,
                                msg.FromID,
                                chatID,//сохраняем с указанным  идентификатором чата
                                msg.ToID,
                                msg.Type,
                                msg.Data,
                                msg.UtcTimestamp),
                            true))
                            MessageAddedToChat?.Invoke(chatID, msg);
                    }
                }
            }
            else//групповое сообщение
            {
                //тут уже есть чат(т.к. сервак делает рассылку только тем, кто в группе состоит)
                var added = await _chatHolder.TryAddUserToChat(msg.FromID, msg.ChatID);// прикрепляем нового юзера к чату(чтобы видеть его у себя)
                if (added)
                    UserAdded?.Invoke(msg.ChatID, msg.FromID);

                if (await _messHolder.SaveMessage(msg,true))
                    MessageAddedToChat?.Invoke(msg.ChatID, msg);
            }
        }
    }
}
