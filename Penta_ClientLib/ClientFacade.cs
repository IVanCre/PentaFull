using Penta_ClientLib.Interfaces;
using MessageLib;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Services;


namespace Penta_ClientLib
{
    // ui->facade->chatManager->webClient  --->
    // ui<-facade<-mesageReciever<-webClient <---
    internal class ClientFacade(
        IAccountManager accManager,
        IChatManager chatManager,
        IMessageReciever messReciever,
        ISettingsHolder settingsProvider,
        IWebClient client
       ) : IClientFacade
    {
        private IAccountManager _accManager=accManager;
        private IChatManager _chatManager=chatManager;
        private ISettingsHolder _settingsProvider=settingsProvider;
        private IMessageReciever _messReciever = messReciever;
        private IWebClient _client = client;


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


        public Task<Tuple<bool, Exception>> Registration(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
                return Task.FromResult(Tuple.Create(false, new Exception("Login should be not null or empty")));
            if (string.IsNullOrEmpty(password))
                return Task.FromResult(Tuple.Create(false, new Exception("Password should be not null or empty")));

            return _accManager.Registration(login, password);
        }
        public Task<Tuple<bool, Exception>> Login(string login, string password) => _accManager.Login(login, password);
        public Task<Tuple<bool, Exception>> DeleteAccount()=>_accManager.DeleteAccount();


        public async Task<string> GetMyContactID()
        {
            var userID = await _settingsProvider.GetValueByName<int>("userID");
            return ContactConverter.ConvertUserIDToContactID(userID);
        }


        public Task<Tuple<bool, Exception>> SendMessageToUser(string userContactID, MessageType type, byte[] data)
        {
            if(string.IsNullOrEmpty(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userContactID should be not null or empty")));

            int recieverUserID = ContactConverter.ExtractUserID(userContactID);
            return  _chatManager.AddMessageToChat(-1, recieverUserID, type, data);
        }
        public Task<Tuple<bool, Exception>> SendMessageToChat(int chatID, MessageType type, byte[] data) => _chatManager.AddMessageToChat(chatID,-1,  type, data);
        public Task<Tuple<bool, Exception>> CreateGroupChat(string chatName)
        {
            if (string.IsNullOrEmpty(chatName))
                return Task.FromResult(Tuple.Create(false, new Exception("chatName should be not null or empty")));

           return _chatManager.SendCreateGroupChat(chatName);
        }
        public Task<int> CreatePrivateChat(string chatName)
        {
            if (string.IsNullOrEmpty(chatName))
               throw new Exception("chatName should be not null or empty");

            return _chatManager.CreatePrivateChat(chatName);
        }
        public Task<Tuple<bool, Exception>> SendResponseToInvite(int chatID,bool accept) => _chatManager.SendResponseToInvite(chatID, accept);
        public Task<Tuple<bool, Exception>> InviteUserToGroupChat(int chatID, string userContactID)
        {
            if (string.IsNullOrEmpty(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userConnectID should be not null or empty")));

            return _chatManager.SendInviteUserToGroupChat(chatID, userContactID);
        }
        public Task<Tuple<bool, Exception>> LeaveGroupChat(int chatID)=>_chatManager.SendLeaveGroupChat(chatID);
        public Task<Tuple<bool, Exception>> DeleteUserFromGroupChat(int chatID, string userContactID)
        {
            if (string.IsNullOrEmpty(userContactID))
                return Task.FromResult(Tuple.Create(false, new Exception("userConnectID should be not null or empty")));

            return _chatManager.SendDeleteUserFromGroupChat(chatID, userContactID);
        }
        public Task<Tuple<bool, Exception>> DeleteGroupChat(int chatID)=>_chatManager.SendDeleteGroupChat(chatID);
        public Task<Tuple<List<ChatInfo>, Exception>> GetAllChatsInfo() => _chatManager.GetAllChatsInfo();



        public ISettingsHolder GetSettings()
        {
            return _settingsProvider;
        }

        public void DisposeClient()
        {
            _client?.Dispose();
        }
    }
}
