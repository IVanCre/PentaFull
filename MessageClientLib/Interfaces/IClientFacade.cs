using MessageLib;
using System.Data;

namespace MessageClientLib.Interfaces
{
    /// <summary>
    /// Хранит булевое значение и ошибку(если есть)
    /// </summary>
    /// <param name="value">значение</param>
    /// <param name="error"></param>
    public sealed class BOOLEANResult(
        bool value,
        Exception error)
    {
        public readonly Exception Error= error;
        public readonly bool Result= value;
    }

    /// <summary>
    /// Хранит int32 значение и ошибку(если есть)
    /// </summary>
    /// <param name="value">значение</param>
    /// <param name="error"></param>
    public sealed class INT32Result(
        int value,
        Exception error)
    {
        public readonly Exception Error=error;
        public readonly int Result=value;
    }


    /// <summary>
    /// Единая точка доступа к функциональности Клиента
    /// </summary>
    public interface IClientFacade
    {
        public Task<BOOLEANResult> Registration(string login, string password);
        public Task<BOOLEANResult> Login(string login, string password);

        /// <summary>
        /// Отдает идентификатор текущего юзера(который работает в клиенте), по которому другие юзеры могут найти 
        /// </summary>
        /// <returns></returns>
        public Task<INT32Result> GetMyContactID();

        /// <summary>
        /// Шлет запрос на удаление всех данных текущего юзера с Сервера.
        /// Внутренние данные так же удаляются с Клиента
        /// </summary>
        /// <returns>Ошибка, если </returns>
        public Task<BOOLEANResult> DeleteAccount();

        /// <summary>
        /// Доступ к настройкам Клиента
        /// </summary>
        /// <returns></returns>
        public ISettingsProvider GetSettings();

        /// <summary>
        /// Сохранить в свои контакты юзера(с идентификатором userID) под именем userName
        /// </summary>
        /// <param name="userName">псевдоним, под которым юзер хранится в контактах</param>
        /// <param name="userID">идентификатор юзера, под которым он существует на сервере</param>
        /// <returns></returns>
        public Task<BOOLEANResult> AddNewUserContact(string userName, int userID);

        /// <summary>
        /// Удалить из контактов юзера 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Task<BOOLEANResult> DeleteUserContact(int userName);


#region GroupChat
        /// <summary>
        /// Создает новый групповой чат(запрос на сервер)
        /// </summary>
        /// <param name="chatName"></param>
        /// <returns>идентификатор созданного чата</returns>
        public Task<INT32Result> CreateGroupChat(string chatName);

        /// <summary>
        /// Приглашаем юзера в наш групповой чат. 
        /// Работает только если текщий Клиент является создателем указанного чата
        /// </summary>
        /// <param name="userID">идентификатора юзера, которому отправим приглашение</param>
        /// <param name="chatName">имя чата, который есть у Клиента</param>
        /// <returns></returns>
        public Task<BOOLEANResult> InviteUserToGroupChat(string userID, string chatName);

        /// <summary>
        /// Юзер, который сейчас работает в Клиенте, шлет запрос на выход из группового чата
        /// </summary>
        /// <param name="chatName">имя чата, который есть у Клиента</param>
        /// <returns></returns>
        public Task<BOOLEANResult> LeaveGroupChat(string chatName);

        /// <summary>
        /// Удаление юзера с из группового чата.
        /// Работает, только если текущий юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatName">имя чата</param>
        /// <param name="userID">идентификатор юзера, которого нужно удалить</param>
        /// <returns></returns>
        public Task<BOOLEANResult> DeleteUserFromGroupChat(string chatName, string userID);

        /// <summary>
        /// Удаляет чат и всю переписку на сервер и на клиенте.
        /// Работает, только если юзер является создателем указанного чата
        /// </summary>
        /// <param name="chatName">имя чата</param>
        /// <returns></returns>
        public Task<BOOLEANResult> DeleteGroupChat(string chatName);
#endregion



        /// <summary>
        /// Удаляет чат(переписку) на Клиенте между 2 юзерами
        /// </summary>
        /// <param name="localChatID">идентификатор чата, под которым он существует на Клиенте</param>
        /// <returns></returns>
        public Task<BOOLEANResult> DeletePrivateChat(int localChatID);

        /// <summary>
        /// Отправка сообщения на сервер
        /// </summary>
        /// <param name="mesage">само сообщение</param>
        /// <returns></returns>
        public Task<BOOLEANResult> SendMessageAsync(Message mesage);

        /// <summary>
        /// Делегат для обработки входящих сообщений от сервера
        /// </summary>
        Func<Message, Task> MessageRecieveAsync { get; set; }
    }
}
