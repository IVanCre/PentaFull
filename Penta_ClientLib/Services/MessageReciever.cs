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
        public event ChatUserListChanged UserAdded;
        public event ChatUserListChanged UserRemoved;
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
                case MessageType.AddUserToGroupResponse:        await ProcessAddUserToGroupResponce(msg); break;
                case MessageType.UserAddedToGroupResponse:      await ProcessAddedToGroupChat(msg);break;
                case MessageType.RemoveUserFromGroupResponce:   await ProcessDeleteUserFromGroupResponce(msg); break;
                case MessageType.CreateGroupResponce:           await ProcessCreateGroupChatResponce(msg);break;
                case MessageType.DeleteGroupResponce:           await ProcessDeleteGroupResponce(msg);break;
                case MessageType.DeleteSelfAccountResponce:     AccountDeleted?.Invoke(); ; break;

                //указанные сообщения сохранятся
                case MessageType.Text:
                case MessageType.Picture:
                case MessageType.Voice:                         await ProcessMessageFromUser(msg);break;
            }
        }


        private Task ProcessAddedToGroupChat(Message msg) => ProcessCreateGroupChatResponce(msg);//создаем у себя чат, в котрорый нас добавили
        private async Task ProcessCreateGroupChatResponce(Message msg)//ответ на запрос
        {
            if (msg.ChatID!=-1)//значит сервак успешно создал
            {
                var chatName = msg.GetDataLikeString();
                if(await _chatHolder.AddGroupChat(msg.ChatID,chatName ))
                    CreatedNewChat?.Invoke(msg.ChatID,chatName);
            }
        }
        private async Task ProcessAddUserToGroupResponce(Message msg)
        {
            if (await _chatHolder.TryAddUserToChat(msg.ToID,msg.ChatID ))//запоминаем юзера, который добавился в чат
                UserAdded?.Invoke(msg.ChatID, msg.ToID);//информируем наверх, что кто-то присоединился к чату,в котором есть мы
        }

        private async Task ProcessDeleteUserFromGroupResponce(Message msg)
        {
            var userIDToDelete = msg.GetDataLikeInt();
            if (await _chatHolder.RemoveUserFromChat(userIDToDelete, msg.ChatID))
            {
                UserRemoved?.Invoke(msg.ChatID, userIDToDelete);
            }
        }        
        
        private async Task ProcessDeleteGroupResponce(Message msg)
        {
            if (await _chatHolder.DeleteChat(msg.ChatID))
            {
                ChatDeleted?.Invoke(msg.ChatID,"");
            }
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
