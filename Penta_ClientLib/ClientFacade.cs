using Penta_ClientLib.Interfaces;
using MessageLib;


namespace Penta_ClientLib
{

    internal class ClientFacade(
        IAccountManager accManager,
        IContactManager contactManager,
        IChatManager chatManager,
        ISettingsHolder settingsProvider
       ) : IClientFacade
    {
        private IAccountManager _accManager=accManager;
        private IContactManager _contactManager=contactManager;
        private IChatManager _chatManager=chatManager;
        private ISettingsHolder _settingsProvider=settingsProvider;

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


        public Task<Tuple<bool,Exception>> Registration(string login, string password)=>_accManager.Registration(login, password);
        public Task<Tuple<bool, Exception>> Login(string login, string password)=>_accManager.Login(login, password);
        public Task<Tuple<bool, Exception>> DeleteAccount()=>_accManager.DeleteAccount();

   
        public Task<Tuple<string, Exception>> GetMyContactID()=> _contactManager.GetMyContactString();
        public Task<Tuple<bool, Exception>> AddNewUserContact(string userName, string userContactID)=>_contactManager.AddNewUserContact(userName, userContactID);
        public Task<Tuple<bool, Exception>> DeleteUserContact(string userName)=>_contactManager.DeleteUserContact(userName);
        public Task<Tuple<List<string>, Exception>> GetAllContacts() => _contactManager.GetAllContacts();

        public Task<Tuple<bool, Exception>> AddMessageToChat(string chatName, string userName, MessageType type, byte[] data) => _chatManager.AddMessageToChat(chatName, userName, type, data);
        public Task<Tuple<bool, Exception>> CreateGroupChat(string chatName)=>_chatManager.SendCreateGroupChat(chatName);
        public Task<Tuple<bool, Exception>> InviteUserToGroupChat(string userID, string chatName)=>_chatManager.SendInviteUserToGroupChat(userID, chatName);
        public Task<Tuple<bool, Exception>> LeaveGroupChat(string chatName)=>_chatManager.SendLeaveGroupChat(chatName);
        public Task<Tuple<bool, Exception>> DeleteUserFromGroupChat(string chatName, string userID)=> _chatManager.SendDeleteUserFromGroupChat(chatName, userID);
        public Task<Tuple<bool, Exception>> DeleteGroupChat(string chatName)=>_chatManager.SendDeleteGroupChat(chatName);



        public ISettingsHolder GetSettings()
        {
            return _settingsProvider;
        }

        public void DisposeClient()
        {
            throw new NotImplementedException();
        }
    }
}
