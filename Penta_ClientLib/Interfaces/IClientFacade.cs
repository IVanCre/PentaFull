
using MessageLib;
using Penta_ClientLib.DataStructs;


namespace Penta_ClientLib.Interfaces
{

    public delegate void NewMessageInChat(int chatID, Message mesage);
    public delegate void ChatChanged(int chatID, string chatName);
    public delegate void ContactChanged(string oldContactName, string newContactName);
    public delegate void ChatUserListChanged(int chatID, int userID);
    public delegate void InvitedToChat(Message msg);
    public delegate void AccountDeleted();
    public delegate void ConnectionStateChanged(bool connected);

    /// <summary>
    /// Единая точка доступа к функциональности Клиента
    /// </summary>
    public interface IClientFacade
    {
        /// <summary>
        /// Вызывается, когда сервер возвращает ответ о создании группового чата
        /// </summary>
        event ChatChanged CreatedNewChat;

        /// <summary>
        /// Вызывается, когда добавляется\удалется\редактируется контакт
        /// </summary>
        event ContactChanged ContactChanged;

        /// <summary>
        /// Вызывается, когда сервер возвращает результат добавления юзера в групповой чат
        /// </summary>
        event ChatUserListChanged UserAdded;

        /// <summary>
        /// Вызывается, когда сервер удаляет юзера из группового чата
        /// </summary>
        event ChatUserListChanged UserRemoved;

        /// <summary>
        /// Вызывается, когда сервер возвращает результат удаления группового чата
        /// </summary>
        event ChatChanged ChatDeleted;

        /// <summary>
        /// Вызывается, когда с сервера приходит новое сообщение в конкретный чат
        /// </summary>
        event NewMessageInChat MessageAddedToChat;

        /// <summary>
        /// Вызывается, когда сервер присылает результат удаления аккаунта(удалет только аккаунт отправителя)
        /// </summary>
        event AccountDeleted AccountDeleted;

        /// <summary>
        /// Вызывается, когда меняется подключение к серверу
        /// </summary>
        event ConnectionStateChanged ConnectionToServerChanged;



        /// <summary>
        /// Регистрация в системе(через сервер)
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Tuple<bool,Exception>> RegistrationAsync(string login, string password);
        Task<Tuple<bool, Exception>> LoginAsync(string login, string password);

        /// <summary>
        /// Подключение к серверу
        /// </summary>
        /// <returns></returns>
        Task<bool> ConnectToServerAsync();

        /// <summary>
        /// Текущее состояние связи с сервером
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Доступ к настройкам Клиента
        /// </summary>
        /// <returns></returns>
        ISettingsProvider GetSettings();

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
        Task<Tuple<bool, Exception>> CreateGroupChatAsync(string chatName);

        /// <summary>
        /// Создает приватный(1на1) чат на клиенте 
        /// </summary>
        /// <param name="chatName"></param>
        /// <returns></returns>
        Task<int> CreatePrivateChatAsync(string chatName);

        /// <summary>
        /// Приглашаем юзера наш групповой чат(через сервер)
        /// Работает только если текщий Клиент является создателем указанного чата
        /// </summary>
        /// <param name="chatID"></param>
        /// <param name="userContactID"></param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> AddUserToGroupChatAsync(int chatID, string userContactID);



        /// <summary>
        /// Юзер, который сейчас работает в Клиенте, шлет запрос на выход из группового чата
        /// </summary>
        /// <param name="chatID">идентификатор чата, который есть у Клиента</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> LeaveGroupChatAsync(int chatID);

        /// <summary>
        /// Удаление юзера с из группового чата.
        /// Работает, только если текущий юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatID">идентификатор чата</param>
        /// <param name="userContactID">идентификатор юзера, которого нужно удалить</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> DeleteUserFromGroupChatAsync(int chatID, string userContactID);

        /// <summary>
        /// Удаляет групповой чат(через сервер)
        /// Работает, только если юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatID">идентификатор чата</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> DeleteGroupChatAsync(int chatID);

        /// <summary>
        /// Удаляет приватный чат(на устройстве)
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        Task<bool> DeletePrivateChatAsync(int chatID);

        /// <summary>
        /// Отдает список всех имеющихся чатов у текущего клиента(групповые и приватные)
        /// </summary>
        /// <returns></returns>
        Task<List<ChatInfo>> GetAllChatsInfoAsync();

        /// <summary>
        /// Отдает найденый чат по его ID
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        Task<ChatInfo> GetChatByID(int chatID);

        /// <summary>
        /// Отправка сообщения в групповой чат
        /// </summary>
        /// <param name="mesage">само сообщение</param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> SendMessageToGroupChatAsync(int chatID, MessageType type, byte[] data);

        /// <summary>
        /// Отправка сообщения в приватный чат
        /// </summary>
        /// <param name="userConnectID">текстовый идентикатор пользователя</param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> SendMessageToUserAsync(int chatID, string userConnectID, MessageType type, byte[] data);//это для клиентов,которые не имеют хранения контактов
        /// <summary>
        /// Отправка сообщения в приватный чат
        /// </summary>
        Task<Tuple<bool, Exception>> SendMessageToUserAsync(int chatID,int userID, MessageType type, byte[] data);

        /// <summary>
        /// Удаление аккаунта на сервере
        /// </summary>
        /// <returns></returns>
        Task<Tuple<bool, Exception>> DeleteAccountAsync();

        /// <summary>
        /// Возвращает указанное количество последних(самых свежих) сообщений из чата
        /// </summary>
        /// <param name="chatID">номер чата</param>
        /// <param name="lastMessageCount">сколько самых свежих сообщений подгрузить</param>
        /// <returns></returns>
        Task<List<Message>> GetMessagesByChatAsync(int chatID, int lastMessageCount);

        /// <summary>
        /// Извлекает из названия приватного чата идентифкатор собеседника
        /// </summary>
        /// <param name="chatName"></param>
        /// <returns></returns>
        Task<int> GetRecieverIDFromChatAsync(string chatName);

        /// <summary>
        /// Добавляет новый контакт
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="contactID"></param>
        /// <returns></returns>
        Task<bool> AddNewContactAsync(string userName, string contactID);

        /// <summary>
        /// Удаление контакта по имени
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> DeleteContactAsync(string userName);

        /// <summary>
        /// Получение списка всех контактов(отсортированны по имени)
        /// </summary>
        /// <returns></returns>
        Task<List<ContactInfo>> GetAllContactsAsync();

        /// <summary>
        /// Являемся ли мы админом-создателем указанного группового чата
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        Task<bool> AmCreatedGroupChat(int chatID);

        /// <summary>
        /// вызывает закрытие всех ресурсов клиента.
        /// Вызывать перед завершением приложения
        /// </summary>
        void DisposeClient();
    }
}
