using Penta_ClientLib.Interfaces;
using MessageLib;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Services;


namespace Penta_ClientLib
{
    // ui->facade->chatManager->(DB)->webClient  --->
    // ui<-facade<-(DB)<-mesageReciever<-webClient <---
    internal class ClientFacade : IClientFacade
    {
        private IAccountManager _accManager;
        private IChatManager _chatManager;
        private ISettingsProvider _settingsProvider;
        private IMessageHolder _messHolder;
        private IContactHolder _contactHolder;
        private IMessageReciever _messReciever;
        private IWebClient _client;

        public ClientFacade(
            IAccountManager accManager,
            IChatManager chatManager,
            IMessageReciever messReciever,
            ISettingsProvider settingsProvider,
            IMessageHolder messHolder,
            IContactHolder contactHolder,
            IWebClient client)
        {
            _accManager = accManager;
            _chatManager = chatManager;
            _settingsProvider = settingsProvider;
            _messHolder = messHolder;
            _contactHolder = contactHolder;
            _messReciever = messReciever;
            _client = client;

            _client.ConnectionStateChanged += SendNonSended;
            _client.MessageSended +=(messageID) => _messHolder.MarkMessageLikeSended(messageID);
        }


        public event ChatChanged CreatedNewChat
        {
            add => _messReciever.CreatedNewChat += value;
            remove=> _messReciever.CreatedNewChat -= value;
        }
        public event ChatChanged ChatDeleted
        {
            add=> _messReciever.ChatDeleted += value;
            remove => _messReciever.ChatDeleted -= value;
        }
        public event ContactChanged ContactChanged;

        public event ChatUserListChanged UserAdded
        {
            add => _messReciever.UserAdded += value;
            remove => _messReciever.UserAdded -= value;
        }
        public event ChatUserListChanged UserRemoved
        {
            add=> _messReciever.UserRemoved += value;
            remove => _messReciever.UserRemoved -= value;
        }
        public event NewMessageInChat MessageAddedToChat
        { 
            add => _messReciever.MessageAddedToChat += value;
            remove=> _messReciever.MessageAddedToChat -= value; 
        }
        public event InvitedToChat RecieveInvite
        {
            add=> _messReciever.RecieveInvite += value;
            remove=> _messReciever.RecieveInvite -= value;
        }
        public event AccountDeleted AccountDeleted
        {
            add => _messReciever.AccountDeleted += value;
            remove => _messReciever.AccountDeleted -= value;
        }
        public event ConnectionStateChanged ConnectionToServerChanged
        {
            add => _client.ConnectionStateChanged += value;
            remove => _client.ConnectionStateChanged -= value;
        }
        


        public Task<Tuple<bool, Exception>> RegistrationAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
                return Task.FromResult(Tuple.Create(false, new Exception("Login should be not null or empty")));
            if (string.IsNullOrEmpty(password))
                return Task.FromResult(Tuple.Create(false, new Exception("Password should be not null or empty")));

            return _accManager.Registration(login, password);
        }
        public Task<bool> ConnectToServerAsync()=>_client.ConnectToMessageHub();

        public Task<Tuple<bool, Exception>> DeleteAccountAsync()=>_accManager.DeleteAccount();


        public Task<string> GetMyContactID() => _settingsProvider.GetCurrentUserContactID();

        public ISettingsProvider GetSettings()
        {
            return _settingsProvider;
        }


        public Task<Tuple<bool, Exception>> SendMessageToUserAsync(int chatID,string userContactID, MessageType type, byte[] data)
        {
            if(string.IsNullOrEmpty(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userContactID should be not null or empty")));

            int recieverUserID = ContactConverter.ExtractUserID(userContactID);
            return  _chatManager.AddMessageToChat(chatID, recieverUserID, type, data);
        }
        public Task<Tuple<bool, Exception>> SendMessageToUserAsync(int chatID,int userID, MessageType type, byte[] data)
        {
            if (userID==-1)
                return Task.FromResult(Tuple.Create(false, new Exception("userContactID should be not null or empty")));

            return _chatManager.AddMessageToChat(chatID, userID, type, data);
        }
        public Task<Tuple<bool, Exception>> SendMessageToGroupChatAsync(int chatID, MessageType type, byte[] data) => _chatManager.AddMessageToChat(chatID,-1,  type, data);
        public Task<Tuple<bool, Exception>> CreateGroupChatAsync(string chatName)
        {
            if (string.IsNullOrEmpty(chatName))
                return Task.FromResult(Tuple.Create(false, new Exception("chatName should be not null or empty")));

           return _chatManager.SendCreateGroupChat(chatName);
        }
        public Task<int> CreatePrivateChatAsync(string chatName)
        {
            if (string.IsNullOrEmpty(chatName))
               throw new Exception("chatName should be not null or empty");

            return _chatManager.CreatePrivateChat(chatName);
        }
        public Task<Tuple<bool, Exception>> SendResponseToInviteAsync(int chatID,bool accept) => _chatManager.SendResponseToInvite(chatID, accept);
        public Task<Tuple<bool, Exception>> InviteUserToGroupChatAsync(int chatID, string userContactID)
        {
            if (string.IsNullOrEmpty(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userConnectID should be not null or empty")));

            return _chatManager.SendInviteUserToGroupChat(chatID, userContactID);
        }
        public Task<Tuple<bool, Exception>> LeaveGroupChatAsync(int chatID)=>_chatManager.SendLeaveGroupChat(chatID);
        public Task<Tuple<bool, Exception>> DeleteUserFromGroupChatAsync(int chatID, string userContactID)
        {
            if (string.IsNullOrEmpty(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userConnectID should be not null or empty")));

            return _chatManager.SendDeleteUserFromGroupChat(chatID, userContactID);
        }
        public Task<Tuple<bool, Exception>> DeleteGroupChatAsync(int chatID)=>_chatManager.SendDeleteGroupChat(chatID);
        public Task<bool> DeletePrivateChatAsync(int chatID) => _chatManager.DeletePrivateChat(chatID);
        public Task<List<ChatInfo>> GetAllChatsInfoAsync() => _chatManager.GetAllChatsInfo();

        public async Task<int> GetRecieverIDFromChatAsync(string chatName)
        {
            int _recieverUserID = -1;
            if (_recieverUserID == -1)
                _recieverUserID = await _contactHolder.GetUserIDByName(chatName);//пробуем тянуть идентификатор юзера из контактов
            if (_recieverUserID == -1)
                _recieverUserID = ContactConverter.ExtractUserID(chatName);//пробуем тянуть идентификатор из самого имени чата

            return _recieverUserID;
        }
        public async Task<bool> AddNewContactAsync(string userName, string contactID)
        {
            var added = await _contactHolder.AddContact(userName, contactID);
            if (added)
                ContactChanged?.Invoke(contactID, userName);

            return added;
        }
        public async Task<bool> DeleteContactAsync(string userName)
        {
            var contactID =await _contactHolder.GetContactIDByName(userName);
            var deleted =await _contactHolder.DeleteByName(userName);
            if(deleted)
                ContactChanged?.Invoke(userName, contactID);//откатываем имя на contactID

            return deleted;
        }
        public async Task<List<ContactInfo>> GetAllContactsAsync()=>await _contactHolder.GetAllContacts();


        public Task<List<Message>> GetMessagesByChatAsync(int chatID, int maxLastMessageCount)=>_messHolder.GetMessagesByChat(chatID, maxLastMessageCount);
        private async void SendNonSended(bool connectionToServer)
        {
            if(connectionToServer)
            {
                try
                {
                    var finded = await _messHolder.GetNonSended();
                    foreach (var msg in finded)
                        await _client.SendMessage(msg);
                }
                catch (Exception ex) { }
            }
        }


        public void DisposeClient()
        {
            _client?.Dispose();
        }
    }
}
