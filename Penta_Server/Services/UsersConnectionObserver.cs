using MessageLib;
using Penta_Server.Interfaces;

namespace Penta_Server.Services
{
    /// <summary>
    /// Уведомляет клиентов, какие юзеры сечас подключены к серверу
    /// </summary>
    public class UsersConnectionObserver: IUsersConnectionObserver
    {
        private IConnectionsRepository _connRepo;
        private IClientNotifier _notifier;
        private ILogWriter _logger;

        /// <summary>
        /// key=recieverID это КТО отслеживает
        /// value.Item1=chatID  в каком чате идет отслеживание
        /// value.Item2=observerID  КОГО отслеживаем
        /// </summary>
        private Dictionary<int, Tuple<int,int>> _observablConn = new();//key=recieverID ,valu=<chatID,observerID> т.к. клиент(reciever) могет отслеживать только 1 экземпляр чата


        public UsersConnectionObserver(
            IClientNotifier notifier,
            IConnectionsRepository connRepo,
            ILogWriter logger)
        {
            _connRepo = connRepo;
            _connRepo.UserConnectionStateChanged += ProcessUserConnectionState;
            _notifier = notifier;
            _logger = logger;
        }

        private void ProcessUserConnectionState(int userID, bool state)
        {
            //_logger.SaveInfo($"Изменилось состояние подключения юзера id={userID} connect={state}.Уведомляем");
            var finded = _observablConn.Where(x => x.Value.Item2 == userID);
            foreach(var item in finded)//всем, кто отслеживает этого юзера шлем сообщение
                _ = _notifier.SendToUserWithoutPush(
                    MessageFactory.UserInSystemState(item.Value.Item1,item.Value.Item2,item.Key, state));

//при отключении юзера удаляем его запрос на отслеживание(если есть)
//нужно(предохранитель) если юзер отключится без предварительного запроса на остановку отслеживания
            if(_observablConn.ContainsKey(userID))
                _observablConn.Remove(userID);
        }



        public void AddToObserve(int chatID, int recieverID, int userIDToObserve)
        {
            if (!_observablConn.ContainsKey(recieverID))
            {
                //_logger.SaveInfo($"Поступил запрос на отслеживание подключения юзера id={userIDToObserve}");
                _observablConn.Add(recieverID, Tuple.Create(chatID, userIDToObserve));
                
                var findedConnection = _connRepo.GetConnectionID(userIDToObserve);
                _notifier.SendToUserWithoutPush(
                    MessageFactory.UserInSystemState(chatID,userIDToObserve, recieverID,!string.IsNullOrEmpty(findedConnection)));
            }
        }

        public void DeleteFromObserve(int chatID, int recieverID, int userIDToObserve)
        {
            //_logger.SaveInfo($"Поступил запрос на прекращение отслеживания подключения юзера id={userIDToObserve}");
            if (_observablConn.ContainsKey(recieverID))
                _observablConn.Remove(recieverID);
        }
    }
}
