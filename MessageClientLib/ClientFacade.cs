using MessageClientLib.Interfaces;
using MessageLib;


namespace MessageClientLib
{

    internal class ClientFacade(
        IAccountManager accManager,
        IMessageProcessor messProcessor,
        IContactManager contactManager,
        IChatManager chatManager,
        ISettingsProvider settingsProvider
       ) : IClientFacade
    {
        private IAccountManager _accManager=accManager;
        private IMessageProcessor _messProcessor = messProcessor;
        private IContactManager _contactManager=contactManager;
        private IChatManager _chatManager=chatManager;
        private ISettingsProvider _settingsProvider=settingsProvider;


        public Func<Message, Task> MessageRecieveAsync
        {
            get => _messProcessor.MessageRecieveAsync;
            set => _messProcessor.MessageRecieveAsync = value;
        } 
        public Task<BOOLEANResult> SendMessageAsync(Message mesage)=> _messProcessor.SendMessage(mesage);


        public Task<BOOLEANResult> Registration(string login, string password)=>_accManager.Registration(login, password);
        public Task<BOOLEANResult> Login(string login, string password)=>_accManager.Login(login, password);
        public Task<BOOLEANResult> DeleteAccount()=>_accManager.DeleteAccount();

   
        public Task<INT32Result> GetMyContactID()=> _contactManager.GetMyContactID();
        public Task<BOOLEANResult> AddNewUserContact(string userName, int userID)=>_contactManager.AddNewUserContact(userName, userID);
        public Task<BOOLEANResult> DeleteUserContact(int userName)=>_contactManager?.DeleteUserContact(userName);


        public Task<INT32Result> CreateGroupChat(string chatName)=>_chatManager.CreateGroupChat(chatName);
        public Task<BOOLEANResult> InviteUserToGroupChat(string userID, string chatName)=>_chatManager.InviteUserToGroupChat(userID, chatName);
        public Task<BOOLEANResult> LeaveGroupChat(string chatName)=>_chatManager.LeaveGroupChat(chatName);
        public Task<BOOLEANResult> DeleteUserFromGroupChat(string chatName, string userID)=> _chatManager.DeleteUserFromGroupChat(chatName, userID);
        public Task<BOOLEANResult> DeleteGroupChat(string chatName)=>_chatManager.DeleteGroupChat(chatName);
        public Task<BOOLEANResult> DeletePrivateChat(int localChatID)=>_chatManager.DeletePrivateChat(localChatID);



        public ISettingsProvider GetSettings()
        {
            return _settingsProvider;
        }
    }
}
