using Penta_ClientLib.Interfaces;
using MessageLib;

namespace Penta_ClientLib.Services
{
    internal class MessageReciever: IMessageReciever
    {
        private IChatHolder _chatHolder;
        private IWebClient _messListener;
        private IMessageHolder _messHolder;
        private IContactHolder _contactHolder;


        public event ChatChanged CreatedNewChat;
        public event ChatChanged ChatDeleted;
        public event ChatUserListChanged UserAdded;
        public event ChatUserListChanged UserRemoved;
        public event NewMessageInChat MessageAddedToChat;
        public event AccountDeleted AccountDeleted;
        public event RecieverConnectedChanged UserConnectionChanged;

        public MessageReciever(
            IWebClient messListener,
            IChatHolder chatProvider,
            IMessageHolder messHolder,
            IContactHolder contactHolder
            )
        {
            _messListener = messListener;
            _messListener.RecievedNewMessage += ProcessResponce;
            _chatHolder = chatProvider;
            _messHolder = messHolder;
            _contactHolder = contactHolder;
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
                case MessageType.DeleteSelfAccountResponce:     AccountDeleted?.Invoke();break;
                case MessageType.UserInSystemState:             UserInSystemChanged(msg);break;  

                //указанные сообщения сохранятся
                case MessageType.SystemNotify:                  await ProcessSystemNotify(msg); break;

                case MessageType.Text:
                case MessageType.Picture:
                case MessageType.Voice:                         await ProcessMessageWithData(msg);break;
            }
        }

        private void UserInSystemChanged(Message msg)
        { 
            var state = msg.GetDataLikeBool();
            if (state != null)
                UserConnectionChanged?.Invoke(msg.ChatID, msg.FromID,state.Value);
        }

        private async Task UserAddedToGroupChat(Message msg)//нас добавили в группу и нам кинули уведомление
        {
            var chatName = msg.GetDataLikeString();
            var finded = await _chatHolder.GetChatIDByName(chatName);
            if (finded == 0)//у нас такого чата нет
            {
                if (await _chatHolder.CreateGroupChat(msg.ChatID, chatName, false))
                    CreatedNewChat?.Invoke(msg.ChatID, chatName);
            }
            UserAdded?.Invoke(msg.ChatID, msg.ToID);
        }

        private async Task CreateGroupChatResponce(Message msg)//ответ на наш запрос(значит мы являемся админом)
        {
            if (msg.ChatID!=-1)//значит сервак успешно создал
            {
                var chatName = msg.GetDataLikeString();
                if(await _chatHolder.CreateGroupChat(msg.ChatID,chatName, true ))
                    CreatedNewChat?.Invoke(msg.ChatID,chatName);
            }
        }

        private void AddUserToGroupResponce(Message msg)//нам ответили, что юзера добавили по нашему запросу
        {
            //UserAdded?.Invoke(msg.ChatID, msg.ToID);
        }
        private void DeleteUserFromGroupResponce(Message msg)
        {
            var userIDToDelete = msg.GetDataLikeInt();
            if(userIDToDelete!=null)
                UserRemoved?.Invoke(msg.ChatID, userIDToDelete.Value);
        }        
        
        private async Task DeleteGroupResponce(Message msg)
        {
            if (await _chatHolder.DeleteChat(msg.ChatID))
                ChatDeleted?.Invoke(msg.ChatID,"");
        }


        private async Task ProcessMessageWithData(Message msg)
        {
            msg.IsSendedToServer = true;
            if (msg.ChatID == -1)//личное сообщение
            {
                int chatID = 0;

                var userName =await _contactHolder.GetUserNameByID(msg.FromID);//ищем контакт, с указанным ID
                if(!string.IsNullOrEmpty(userName))
                    chatID= await _chatHolder.GetChatIDByName(userName);//ищем чат, у которого имя соответсвует имю из контакта

                if (chatID == 0)//поиск по контакту не дал результата(контакта нет?)
                    chatID = await _chatHolder.GetChatIDByName(ContactConverter.ConvertUserIDToContactID(msg.FromID));//ищем по самому contactID(это дефолтное)

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
                            msg.UtcTimestamp,
                            msg.IsSendedToServer),
                        true))

                        MessageAddedToChat?.Invoke(chatID, msg);
                }
                else//нет чата с указанным connectID
                {
                    var userConnectionID = ContactConverter.ConvertUserIDToContactID(msg.FromID);
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
                                msg.UtcTimestamp,
                                msg.IsSendedToServer),
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

        private async Task ProcessSystemNotify(Message msg)//для системных уведомлений
        {
            msg.IsSendedToServer = true;
            var chatID = await _chatHolder.GetChatIDByName("Системные оповещения");
            if (chatID == 0)
            {
                chatID = await _chatHolder.CreateReadOnlyChat("Системные оповещения");
                if (chatID != 0)
                    CreatedNewChat?.Invoke(chatID, "Системные оповещения");
            }

            if (chatID != 0 && await _messHolder.SaveMessage(
                new Message(
                    msg.ID,
                    msg.FromID,
                    chatID,
                    msg.ToID,
                    msg.Type,
                    msg.Data,
                    msg.UtcTimestamp,
                    msg.IsSendedToServer),
                true))
                MessageAddedToChat?.Invoke(chatID, msg);
        }
    }
}
