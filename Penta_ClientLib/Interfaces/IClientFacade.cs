
using MessageLib;
using Penta_ClientLib.DataStructs;


namespace Penta_ClientLib.Interfaces
{

    public delegate void NewMessageInChat(int chatID, Message mesage);
    public delegate void ChatChanged(int chatID, string chatName);
    public delegate void ChatUserListChanged(int chatID, int userID);
    public delegate void InvitedToChat(Message msg);
    public delegate void AccountDeleted();

    /// <summary>
    /// Единая точка доступа к функциональности Клиента
    /// </summary>
    public interface IClientFacade
    {
        /// <summary>
        /// Регистрация в системе(через сервер)
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Tuple<bool,Exception>> Registration(string login, string password);

        /// <summary>
        /// Вход в систему(через сервер)
        /// </summary>
        /// <param name="login">если не указан- будет взят из хранилища</param>
        /// <param name="password">если не указан- будет взят из хранилища</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> Login(string login=null, string password = null);




        /// <summary>
        /// Доступ к настройкам Клиента
        /// </summary>
        /// <returns></returns>
        ISettingsHolder GetSettings();

        /// <summary>
        /// Получение идентификатора юзера, для возможности идентификации юзера в системе
        /// </summary>
        /// <returns></returns>
        Task<string> GetMyContactID();

        /// <summary>
        /// Создает новый групповой чат(через сервер)
        /// </summary>
        /// <param name="chatName"></param>
        /// <returns>факт отправки запроса</returns>
        Task<Tuple<bool, Exception>> CreateGroupChat(string chatName);

        /// <summary>
        /// Вызывается, когда сервер возвращает ответ о создании группового чата
        /// </summary>
        event ChatChanged CreatedNewChat;

        /// <summary>
        /// Создает приватный(1на1) чат на клиенте 
        /// </summary>
        /// <param name="chatName"></param>
        /// <returns></returns>
        Task<int> CreatePrivateChat(string chatName);

        /// <summary>
        /// Приглашаем юзера наш групповой чат(через сервер)
        /// Работает только если текщий Клиент является создателем указанного чата
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="userContactID"></param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> InviteUserToGroupChat(int chatID, string userContactID);

        /// <summary>
        /// Вызывается, когда сервер возвращает результат добавления юзера в групповой чат
        /// </summary>
        event ChatUserListChanged UserAdded;

        /// <summary>
        /// Отправка ответа на приглашение в групповой чат(через сервер)
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="acceptInvite"></param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> SendResponseToInvite(int chatID, bool acceptInvite);

        /// <summary>
        /// Юзер, который сейчас работает в Клиенте, шлет запрос на выход из группового чата
        /// </summary>
        /// <param name="chatID">идентификатор чата, который есть у Клиента</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> LeaveGroupChat(int chatID);

        /// <summary>
        /// Вызывается, когда сервер удаляет юзера из группового чата
        /// </summary>
        event ChatUserListChanged UserRemoved;

        /// <summary>
        /// Удаление юзера с из группового чата.
        /// Работает, только если текущий юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatID">идентификатор чата</param>
        /// <param name="userContactID">идентификатор юзера, которого нужно удалить</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> DeleteUserFromGroupChat(int chatID, string userContactID);

        /// <summary>
        /// Удаляет групповой чат(через сервер)
        /// Работает, только если юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatID">идентификатор чата</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> DeleteGroupChat(int chatID);

        /// <summary>
        /// Вызывается, когда сервер возвращает результат удаления группового чата
        /// </summary>
        event ChatChanged ChatDeleted;

        /// <summary>
        /// Отдает список всех имеющихся чатов у текущего клиента(групповые и приватные)
        /// </summary>
        /// <returns></returns>
        Task< Tuple< List<ChatInfo>,Exception> > GetAllChatsInfo();



        /// <summary>
        /// Отправка сообщения в групповой чат
        /// </summary>
        /// <param name="mesage">само сообщение</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> SendMessageToChat(int chatID, MessageType type, byte[] data);

        /// <summary>
        /// Отправка сообщения в приватный чат
        /// </summary>
        /// <param name="userConnectID"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> SendMessageToUser(string userConnectID, MessageType type, byte[] data);

        /// <summary>
        /// Вызывается, когда с сервера приходит новое сообщение в конкретный чат
        /// </summary>
        event NewMessageInChat MessageAddedToChat;

        /// <summary>
        /// Вызывается, когда с сервера приходит приглашение(от админа группы) на вступление в групповой чат
        /// </summary>
        event InvitedToChat RecieveInvite;

        /// <summary>
        /// Удаление аккаунта на сервере
        /// </summary>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> DeleteAccount();

        /// <summary>
        /// Вызывается, когда сервер присылает результат удаления аккаунта(удалет только аккаунт отправителя)
        /// </summary>
        event AccountDeleted AccountDeleted;

        /// <summary>
        /// вызывает закрытие всех ресурсов клиента.
        /// Вызывать перед завершением приложения
        /// </summary>
        void DisposeClient();
    }
}
