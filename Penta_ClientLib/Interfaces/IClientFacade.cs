
using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    public delegate void NewMessageInChat(int chatID, Message mesage);
    public delegate void ChatChanged(int chatID);
    public delegate void ChatUserListChanged(int chatID, int userID);


    /// <summary>
    /// Единая точка доступа к функциональности Клиента
    /// </summary>
    public interface IClientFacade
    {
        /// <summary>
        /// Регистрация в системе
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<Tuple<bool,Exception>> Registration(string login, string password);

        /// <summary>
        /// Вход в систему
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> Login(string login, string password);

        /// <summary>
        /// Отдает идентификатор текущего юзера,
        /// по которому другие юзеры могут отправлять ему сообщения
        /// </summary>
        /// <returns></returns>
        public Task<Tuple<string, Exception>> GetMyContactID();

        /// <summary>
        /// Шлет запрос на удаление всех данных текущего юзера с Сервера.
        /// Внутренние данные так же удаляются с Клиента
        /// </summary>
        /// <returns>Ошибка, если </returns>
        public Task<Tuple<bool, Exception>> DeleteAccount();

        /// <summary>
        /// Доступ к настройкам Клиента
        /// </summary>
        /// <returns></returns>
        public ISettingsHolder GetSettings();


        /// <summary>
        /// Сохранить в свои контакты юзера(с идентификатором userID) под именем userName
        /// </summary>
        /// <param name="userName">псевдоним, под которым юзер хранится в контактах</param>
        /// <param name="userContactID">идентификатор юзера, который генерирует его Клиент</param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> AddNewUserContact(string userName, string userContactID);

        /// <summary>
        /// Удалить из контактов юзера 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> DeleteUserContact(string userName);

        public Task<Tuple<List<string>, Exception>> GetAllContacts();


#region GroupChat
        /// <summary>
        /// Создает новый групповой чат(запрос на сервер)
        /// </summary>
        /// <param name="chatName"></param>
        /// <returns>факт отправки запроса</returns>
        public Task<Tuple<bool, Exception>> CreateGroupChat(string chatName);
        public event ChatChanged CreatedNewChat;

        /// <summary>
        /// Приглашаем юзера в наш групповой чат. 
        /// Работает только если текщий Клиент является создателем указанного чата
        /// </summary>
        /// <param name="userID">идентификатора юзера, которому отправим приглашение</param>
        /// <param name="chatName">имя чата, который есть у Клиента</param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> InviteUserToGroupChat(string userID, string chatName);
        public event ChatUserListChanged UserAdded;

        /// <summary>
        /// Юзер, который сейчас работает в Клиенте, шлет запрос на выход из группового чата
        /// </summary>
        /// <param name="chatName">имя чата, который есть у Клиента</param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> LeaveGroupChat(string chatName);
        public event ChatUserListChanged UserRemoved;

        /// <summary>
        /// Удаление юзера с из группового чата.
        /// Работает, только если текущий юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatName">имя чата</param>
        /// <param name="userID">идентификатор юзера, которого нужно удалить</param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> DeleteUserFromGroupChat(string chatName, string userID);

        /// <summary>
        /// Удаляет чат и всю переписку на сервер и на клиенте.
        /// Работает, только если юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatName">имя чата</param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> DeleteGroupChat(string chatName);
        public event ChatChanged ChatDeleted;

        #endregion


        /// <summary>
        /// Отправка сообщения на сервер
        /// </summary>
        /// <param name="mesage">само сообщение</param>
        /// <returns></returns>
        public Task<Tuple<bool, Exception>> AddMessageToChat(string chatName, string userName, MessageType type, byte[] data);

        /// <summary>
        /// Делегат для отслеживания появления сообщений в чате(своих и чужих)
        /// </summary>
        public event NewMessageInChat MessageAddedToChat;

        /// <summary>
        /// вызывает закрытие всех ресурсов клиента.
        /// Вызывать перед завершением приложения
        /// </summary>
        public void DisposeClient();
    }
}
