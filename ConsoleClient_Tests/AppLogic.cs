using MessageLib;
using Microsoft.Extensions.DependencyInjection;
using Penta_ClientLib.Interfaces;


namespace ConsoleClient_Tests
{
    internal static class AppLogic
    {
        private static void ChatDeleted(int chatID,string chatName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">> Чат id={chatID} удален на сервере");//тут уже верхний слой принимает решение что делать со своим экземпляра чата
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void ChatCreated(int chatID, string chatName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">> Чат id={chatID} создан");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void MessageAddedToChat(int chatID, Message mesage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">> В чат id={chatID} добавлено новое сообщение: '{mesage.GetDataLikeString()}'");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void UserAddedToChat(int chatID, int userID)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">> Юзер id={userID} добавлен в чат id={chatID}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void UserRemovedFromChat(int chatID, int userID)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">> Юзер id={userID} удален из чата id={chatID}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void RecieveInvite(Message msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">>Получено приглашение для вступления в чат: {msg.ChatID}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void DeletedAcсount()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">> Ваш аккаунт удален");//тут уже верхний слой принимает решение что делать со своим экземпляра чата
            Console.ForegroundColor = ConsoleColor.White;
        }


        private static bool EnterToSystem(IClientFacade facade)
        {
            Console.WriteLine("Регистрация(R) или Вход(L)");
            string enter = Console.ReadLine();

            Console.WriteLine("Ведите логин и пароль");
            string login = Console.ReadLine();
            string pass = Console.ReadLine();

            Tuple<bool, Exception> result = default;
            switch (enter)
            {
                case "R": result = facade.Registration(login, pass).Result; break;
                case "L": result = facade.Login(login, pass).Result; break;
            }
            Console.WriteLine($"Вход выполнен:{result.Item1}");
            return result.Item1;
        }

       

        private static void SendMessage(IClientFacade facade, bool toGroupChat)
        {
            Console.WriteLine("сообщение:");
            string text = Console.ReadLine();

            if (toGroupChat)
            {
                Console.WriteLine("ID чата:");
                string chatName = Console.ReadLine();
                facade.SendMessageToChat(int.Parse(chatName), MessageType.Text, MessageUtils.TextToBytes(text));
            }
            else
            {
                Console.WriteLine("connectID получателя:");
                string userConnectID = Console.ReadLine();
                facade.SendMessageToUser(userConnectID, MessageType.Text, MessageUtils.TextToBytes(text));
            }
        }

        private static async void GetAllChats(IClientFacade facade)
        {
            var finded = await facade.GetAllChatsInfo();
            foreach (var item in finded.Item1)
                Console.WriteLine($"ChatID={item.ID} ChatName={item.Name}");
        }
        private static void ResponseToInviteChat(IClientFacade facade)
        {
            Console.WriteLine("ID чата для вступления:");
            var chatID= Console.ReadLine();
            Console.WriteLine("согласны Y|N:");
            var symbol = Console.ReadLine();

            _=facade.SendResponseToInvite(int.Parse(chatID), symbol=="Y");
        }


        public static void WorkLoop(IServiceProvider services)
        {
            try
            {
                var facade = services.GetRequiredService<IClientFacade>();

                facade.CreatedNewChat += ChatCreated;
                facade.UserAdded += UserAddedToChat;
                facade.UserRemoved += UserRemovedFromChat;
                facade.RecieveInvite += RecieveInvite;
                facade.ChatDeleted += ChatDeleted;
                facade.MessageAddedToChat += MessageAddedToChat;
                facade.AccountDeleted += DeletedAcсount;


                if (EnterToSystem(facade))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ваш контактный номер: {facade.GetMyContactID().Result}");
                    Console.ForegroundColor = ConsoleColor.White;
                    ActionSelector(facade);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }

        private static void ActionSelector(IClientFacade facade)
        {
            string input = "";
            while (input != "-1")//цикл возможных действий
            {
                Console.WriteLine(

                    "4- создать групповой чат(chatName)\n" +
                    "5- пригласить в чат(userContactID,chatID)\n" +
                    "6- удалить из чата юзера(chatID,userContactID)\n" +
                    "7- покинуть чат самому(chatID)\n" +
                    "8- удалить чат(chatID)\n" +

                    "9- написать в групповой чат\n" +
                    "10-написть юзеру\n" +
                    "12-получить список Чатов\n" +
                    "13-ответить на приглашение(chatID, Y-N )\n" +
                    "14-удалить свой аккаунт\n");

                input = Console.ReadLine();
                switch (input)
                {

                    case "4":
                        {
                            var chatName = Console.ReadLine();
                            facade.CreateGroupChat(chatName);
                        }
                        break;
                    case "5":
                        {
                            var userContactID = Console.ReadLine(); 
                            var chatID = Console.ReadLine();                     
                            facade.InviteUserToGroupChat(int.Parse(chatID), userContactID);
                        }
                        break;
                    case "6":
                        {
                            var chatID = Console.ReadLine();
                            var userContactID = Console.ReadLine();
                            facade.DeleteUserFromGroupChat(int.Parse(chatID), userContactID);
                        }
                        break;
                    case "7":
                        {
                            var chatID = Console.ReadLine();
                            facade.LeaveGroupChat(int.Parse(chatID));
                        }
                        break;
                    case "8":
                        {
                            var chatID = Console.ReadLine();
                            facade.DeleteGroupChat(int.Parse(chatID));
                        }
                        break;

                    case "9": SendMessage(facade, true); break;
                    case "10": SendMessage(facade, false); break;
                    case "12": GetAllChats(facade); break;
                    case "13": ResponseToInviteChat(facade); break;
                    case "14": facade.DeleteAccount();break;
                }
            }
        }
    }
}
