using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Penta_ClientLib;
using Penta_ClientLib.Interfaces;
using MessageLib;


namespace ConsoleClient_Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddClientDependencies();

            var host = builder.Build();

            StartLoop(host.Services);

            host.RunAsync();
        }



        public static void ChatDeleted(int chatID)
        {
            Console.WriteLine($">> Чат id={chatID} удален");
        }
        public static void ChatCreated(int chatID)
        {
            Console.WriteLine($">> Чат id={chatID} создан");
        }
        public static void MessageAddedToChat(int chatID, Message mesage)
        {
            Console.WriteLine($">> В чат id={chatID} добавлено новое сообщение: '{mesage.GetDataLikeString()}'");
        }
        public static void UserAddedToChat(int chatID, int userID)
        {
            Console.WriteLine($">> Юзер id={userID} добавлен в чат id={chatID}");
        }
        public static void UserRemovedFromChat(int chatID, int userID)
        {
            Console.WriteLine($">> Юзер id={userID} удален из чата id={chatID}");
        }


        public static void StartLoop(IServiceProvider services)
        {
            var facade = services.GetRequiredService<IClientFacade>();

            facade.ChatDeleted += ChatDeleted;
            facade.CreatedNewChat += ChatCreated;
            facade.MessageAddedToChat += MessageAddedToChat;
            facade.UserAdded += UserAddedToChat;
            facade.UserRemoved += UserRemovedFromChat;

            Console.WriteLine("Регистрация(R) или Вход(L)");
            string enter= Console.ReadLine();
            Console.WriteLine("Ведите логин и пароль");
            string login= Console.ReadLine();
            string pass = Console.ReadLine();
            switch(enter)
            {
                case "R": facade.Registration(login,pass);break;
                case "L": facade.Login(login,pass);break;
            }

            Console.WriteLine($"Ваш контактный номер: {facade.GetMyContactID().Result}");

            string input = "";
            while(input!="-1")//цикл возможных действий
            {
                Console.WriteLine(
                    "1- добавить контакт\n" +
                    "2- получить все контакты\n" +
                    "");
            }
        }

    }
}
