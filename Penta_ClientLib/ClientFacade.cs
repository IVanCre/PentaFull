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
        private IWebClient _webClient;
        private ILogger _logger;

        public ClientFacade(
            IAccountManager accManager,
            IChatManager chatManager,
            IMessageReciever messReciever,
            ISettingsProvider settingsProvider,
            IMessageHolder messHolder,
            IContactHolder contactHolder,
            IWebClient client,
            ILogger logger)
        {
            _accManager = accManager;
            _chatManager = chatManager;
            _settingsProvider = settingsProvider;
            _messHolder = messHolder;
            _contactHolder = contactHolder;
            _messReciever = messReciever;
            _webClient = client;
            _logger = logger;

            _webClient.ConnectionStateChanged += SendNonSended;
            _webClient.MessageSended +=(messageID) => _messHolder.MarkMessageLikeSended(messageID);
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

        public event RecieverConnectedChanged RecieverConnectedChanged
        {
            add=>_messReciever.UserConnectionChanged += value;
            remove=>_messReciever.UserConnectionChanged -= value;
        }

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
        public event AccountDeleted AccountDeleted
        {
            add => _messReciever.AccountDeleted += value;
            remove => _messReciever.AccountDeleted -= value;
        }

        public event MessageSended MessageSendedOnServer
        {
            add => _webClient.MessageSended += value;
            remove => _webClient.MessageSended -= value;
        }
        public event ConnectionStateChanged ConnectionToServerChanged
        {
            add => _webClient.ConnectionStateChanged += value;
            remove => _webClient.ConnectionStateChanged -= value;
        }

        public event SysLogRecieved SysLogRecieved
        {
            add=> _logger.SysLogRecieved += value;
            remove => _logger.SysLogRecieved -= value;
        }


        public Task<Tuple<bool, Exception>> RegistrationAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
                return Task.FromResult(Tuple.Create(false, new Exception("Login should be not null or empty")));
            if (string.IsNullOrEmpty(password))
                return Task.FromResult(Tuple.Create(false, new Exception("Password should be not null or empty")));

            return _accManager.RegistrationAsync(login, password);
        }
        public Task<Tuple<bool, Exception>> LoginAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
                return Task.FromResult(Tuple.Create(false, new Exception("Login should be not null or empty")));
            if (string.IsNullOrEmpty(password))
                return Task.FromResult(Tuple.Create(false, new Exception("Password should be not null or empty")));

            return _accManager.LoginAsync(login, password);
        }


        public Task<bool> ConnectToServerAsync()=>_webClient.ConnectToMessageHub();
        public bool IsConnected() => _webClient.IsConnected();

        public Task<Tuple<bool, Exception>> DeleteAccountAsync()=>_accManager.DeleteAccount();


        public Task<string> GetMyContactID() => _settingsProvider.GetCurrentUserContactID();

        public ISettingsProvider GetSettings()
        {
            return _settingsProvider;
        }


        public Task<Tuple<bool, Exception>> SendMessageToUserAsync(int chatID,string userContactID, MessageType type, byte[] data, long? messageID)
        {
            if(!ContactConverter.ContactIdValid(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userContactID should be not null or empty")));

            int recieverUserID = ContactConverter.ExtractUserID(userContactID);
            return  _chatManager.AddMessageToChat(chatID, recieverUserID, type, data, messageID);
        }
        public Task<Tuple<bool, Exception>> SendMessageToUserAsync(int chatID,int userID, MessageType type, byte[] data, long? messageID)
        {
            if (userID==-1)
                return Task.FromResult(Tuple.Create(false, new Exception("userContactID should be not null or empty")));

            return _chatManager.AddMessageToChat(chatID, userID, type, data, messageID);
        }
        public Task<Tuple<bool, Exception>> SendMessageToGroupChatAsync(int chatID, MessageType type, byte[] data, long? messageID) => _chatManager.AddMessageToChat(chatID,-1,  type, data, messageID);
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
       

        public Task<Tuple<bool, Exception>> AddUserToGroupChatAsync(int chatID, string userContactID)
        {
            if (!ContactConverter.ContactIdValid(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userConnectID invalid struct")));

            return _chatManager.SendAddUserToGroupChat(chatID, userContactID);
        }
        public Task<Tuple<bool, Exception>> LeaveGroupChatAsync(int chatID)=>_chatManager.SendLeaveGroupChat(chatID);
        public Task<Tuple<bool, Exception>> DeleteUserFromGroupChatAsync(int chatID, string userContactID)
        {
            if (!ContactConverter.ContactIdValid(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userConnectID should be not null or empty")));

            return _chatManager.SendDeleteUserFromGroupChat(chatID, userContactID);
        }
        public Task<Tuple<bool, Exception>> DeleteGroupChatAsync(int chatID)=>_chatManager.SendDeleteGroupChat(chatID);
        public Task<bool> DeletePrivateChatAsync(int chatID) => _chatManager.DeletePrivateChat(chatID);
        public Task<List<ChatInfo>> GetAllChatsInfoAsync() => _chatManager.GetAllChatsInfo();
        public Task<ChatInfo> GetChatByID(int chatID) => _chatManager.GetChatByID(chatID);
        public Task<bool> AmCreatedGroupChat(int chatID)=>_chatManager.AmCreatedThisGroupChat(chatID);


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
            if (ContactConverter.ContactIdValid(contactID))
            {
                var added = await _contactHolder.AddContact(userName, contactID);
                if (added)
                    ContactChanged?.Invoke(contactID, userName);

                return added;
            }
            return false;
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
        public async Task<string> FindUserPseudonimeByID(int userID)
        {
            string userName = await _contactHolder.GetUserNameByID(userID);
            if (string.IsNullOrEmpty(userName))
            {
                if (userID == -1)//такой отправитель может быть только у Системы.
                    userName = "Система";
                else
                    userName = ContactConverter.ConvertUserIDToContactID(userID);
            }
            return userName;
        }


        public Task<List<Message>> GetOldMessagesByChatAsync(int chatID, int maxLastMessageCount,DateTimeOffset startTimestamp)=>
            _messHolder.GetLastMessagesByChat(chatID, maxLastMessageCount, startTimestamp);

        private async void SendNonSended(bool connectionToServer)
        {
            if(connectionToServer)
            {
                try
                {
                    var finded = await _messHolder.GetNonSended();
                    foreach (var msg in finded)
                        await _webClient.SendMessage(msg);
                }
                catch (Exception ex) { }
            }
        }


        public void UseSysLogger(bool canWork) => _logger.CanUseLogs(canWork);


        public void DisposeClient()
        {
            _webClient?.Dispose();
        }

        public async void StartObserveUserConnection(int chatID, int observerUserID)
        {
            int currentUserID = await _settingsProvider.GetUserID();
            _ =await _webClient?.SendMessage(MessageFactory.StartObserveUserInSystem(currentUserID, chatID,observerUserID));
        }

        public async void EndObserveUserConnection(int chatID, int observerUserID)
        {
            int currentUserID = await _settingsProvider.GetUserID();
            _ =await _webClient?.SendMessage(MessageFactory.EndObserveUserInSystem(currentUserID,chatID, observerUserID));
        }
    }
}
