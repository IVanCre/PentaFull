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

            bool enterComplete = false;
            switch (enter)
            {
                case "R":
                    {
                        Console.WriteLine("Ведите логин и пароль");
                        string login = Console.ReadLine();
                        string pass = Console.ReadLine();
                        if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(pass))
                        {
                            var result = facade.RegistrationAsync(login, pass).Result;//потому что в консоли рабоатем-это 1 поток
                            enterComplete = result.Item1;
                        }
                        else
                            Console.WriteLine("Некорректные логин или пароль");

                        break;
                    }
                case "L":
                    {
                        enterComplete = facade.ConnectToServerAsync().Result;
                        break;
                    }
            }
            Console.WriteLine($"Вход выполнен:{enterComplete}");
            return enterComplete;
        }

       

        private static void SendMessage(IClientFacade facade, bool toGroupChat)
        {
            Console.WriteLine("сообщение:");
            string text = Console.ReadLine();


            if (toGroupChat)
            {
                Console.WriteLine("ID чата:");
                string chatName = Console.ReadLine();
                facade.SendMessageToGroupChatAsync(int.Parse(chatName), MessageType.Text, MessageUtils.TextToBytes(text),null);
            }
            else
            {
                Console.WriteLine("connectID получателя:");
                string userConnectID = Console.ReadLine();
                facade.SendMessageToUserAsync(-1,userConnectID, MessageType.Text, MessageUtils.TextToBytes(text),null);
            }
        }

        private static async void GetAllChats(IClientFacade facade)
        {
            var finded = await facade.GetAllChatsInfoAsync();
            foreach (var item in finded)
                Console.WriteLine($"ChatID={item.ID} ChatName={item.ChatName}");
        }


        public static async void WorkLoop(IServiceProvider services)
        {
            try
            {
                var facade = services.GetRequiredService<IClientFacade>();

                facade.CreatedNewChat += ChatCreated;
                facade.UserAdded += UserAddedToChat;
                facade.UserRemoved += UserRemovedFromChat;
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
                    "14-удалить свой аккаунт\n");

                input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    switch (input)
                    {

                        case "4":
                            {
                                var chatName = Console.ReadLine();
                                facade.CreateGroupChatAsync(chatName);
                            }
                            break;
                        case "5":
                            {
                                var userContactID = Console.ReadLine();
                                var chatID = Console.ReadLine();
                                facade.AddUserToGroupChatAsync(int.Parse(chatID), userContactID);
                            }
                            break;
                        case "6":
                            {
                                var chatID = Console.ReadLine();
                                var userContactID = Console.ReadLine();
                                facade.DeleteUserFromGroupChatAsync(int.Parse(chatID), userContactID);
                            }
                            break;
                        case "7":
                            {
                                var chatID = Console.ReadLine();
                                facade.LeaveGroupChatAsync(int.Parse(chatID));
                            }
                            break;
                        case "8":
                            {
                                var chatID = Console.ReadLine();
                                facade.DeleteGroupChatAsync(int.Parse(chatID));
                            }
                            break;

                        case "9": SendMessage(facade, true); break;
                        case "10": SendMessage(facade, false); break;
                        case "12": GetAllChats(facade); break;
                        case "14": facade.DeleteAccountAsync(); break;
                    }
                }
            }
        }
    }
}
