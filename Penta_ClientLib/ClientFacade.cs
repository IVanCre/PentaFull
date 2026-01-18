using Penta_ClientLib.Interfaces;
using Penta_ClientLib.MethodResults;
using MessageLib;


namespace Penta_ClientLib
{

    internal class ClientFacade(
        IAccountManager accManager,
        IContactManager contactManager,
        IChatManager chatManager,
        ISettingsProvider settingsProvider
       ) : IClientFacade
    {
        private IAccountManager _accManager=accManager;
        private IContactManager _contactManager=contactManager;
        private IChatManager _chatManager=chatManager;
        private ISettingsProvider _settingsProvider=settingsProvider;

        public event ChatChanged CreatedNewChat
        {
            add =>  _chatManager.CreatedNewChat += value;
            remove=>_chatManager.CreatedNewChat -= value;
        }
        public event ChatChanged ChatDeleted
        {
            add=>     _chatManager.ChatDeleted += value;
            remove => _chatManager.ChatDeleted -= value;
        }
        public event ChatUserListChanged UserAdded
        {
            add =>    _chatManager.UserAdded += value;
            remove => _chatManager.UserAdded -= value;
        }
        public event ChatUserListChanged UserRemoved
        {
            add=>     _chatManager.UserRemoved += value;
            remove => _chatManager.UserRemoved -= value;
        }

        public event NewMessageInChat MessageAddedToChat
        { 
            add => _chatManager.MessageAddedToChat += value;
            remove=> _chatManager.MessageAddedToChat -= value; 
        }


        public Task<BOOLResult> Registration(string login, string password)=>_accManager.Registration(login, password);
        public Task<BOOLResult> Login(string login, string password)=>_accManager.Login(login, password);
        public Task<BOOLResult> DeleteAccount()=>_accManager.DeleteAccount();

   
        public Task<STRResult> GetMyContactID()=> _contactManager.GetMyContactString();
        public Task<BOOLResult> AddNewUserContact(string userName, string userContactID)=>_contactManager.AddNewUserContact(userName, userContactID);
        public Task<BOOLResult> DeleteUserContact(string userName)=>_contactManager?.DeleteUserContact(userName);


        public Task<BOOLResult> AddMessageToChat(string chatName, string userName, MessageType type, byte[] data) => _chatManager.AddMessageToChat(chatName, userName, type, data);
        public Task<BOOLResult> CreateGroupChat(string chatName)=>_chatManager.SendCreateGroupChat(chatName);
        public Task<BOOLResult> InviteUserToGroupChat(string userID, string chatName)=>_chatManager.SendInviteUserToGroupChat(userID, chatName);
        public Task<BOOLResult> LeaveGroupChat(string chatName)=>_chatManager.SendLeaveGroupChat(chatName);
        public Task<BOOLResult> DeleteUserFromGroupChat(string chatName, string userID)=> _chatManager.SendDeleteUserFromGroupChat(chatName, userID);
        public Task<BOOLResult> DeleteGroupChat(string chatName)=>_chatManager.SendDeleteGroupChat(chatName);



        public ISettingsProvider GetSettings()
        {
            return _settingsProvider;
        }

        public void DisposeClient()
        {
            throw new NotImplementedException();
        }
    }
}
