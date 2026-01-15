

using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text.Json;
using MessageLib;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;



namespace SignalRClient
{
    internal class Program
    {
        private static string url= "https://192.168.1.35:9093";

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("-Введите свое имя и пароль: ");
                string user = Console.ReadLine();
                string pass = Console.ReadLine();

                Console.WriteLine("-Регистрация(R) -Вход(L) -Удаление(D):");
                string enetrType = Console.ReadLine();

                string jwtToken = string.Empty;
                switch (enetrType)
                {
                    case "R": jwtToken = await Registration(user, pass); break;
                    case "L": jwtToken = await Login(user, pass); break;
                    case "D":
                        {
                            jwtToken = await Login(user, pass);
                            DeleteAccount(jwtToken);
                            break;
                        }
                }
                 
                Console.WriteLine("-----------------------------");


                if (enetrType != "D")
                {
                    if (!string.IsNullOrEmpty(jwtToken))
                    {
                        var connection = CreateSignalRConnection(jwtToken);
                        await Start(connection);

                        await SendLoop(GetUserID(jwtToken), connection);

                        await Stop(connection);

                        // StartListenThread(jwtToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);

            }
                Console.ReadLine();
        }

        public static int GetUserID(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(jwtToken))
            {
                return -1; // Token is not a valid JWT format
            }

            var token = handler.ReadJwtToken(jwtToken);
            return int.Parse(token.Claims.FirstOrDefault(x => x.Type == "userID")?.Value);
        }


        #region client_methods
        private static HubConnection CreateSignalRConnection(string jwtToken)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{url}/chat", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(jwtToken);//передаем токен,чтоб пропустили на хаб
                    options.HttpMessageHandlerFactory = _ =>
                    {
                        return HandlerCustomCertCheck();
                    };
                })
                .Build();

            // Обработка входящих сообщений
            connection.On<Message>("RecieveMessage", async (message) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nFrom {message.FromUserID}: {message.Data}");
                Console.ForegroundColor = ConsoleColor.White;

                await connection.InvokeAsync("AcknowledgeReceived", message.ID);//подтверждение о получении
            });

            return connection;
        }

        private static async Task<string> Login(string user, string pass)
        {
            string token = "";
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var fullUrl = $"{url}/User/Login?name={user}&pass={pass}";

                var response = await httpClient.GetAsync(fullUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    token = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("-Вход завершен");
                }
                else
                    Console.WriteLine("-Сервер отверг вход");
            }
            Console.WriteLine("-Ваш токен: " + token);

            return token;
        }
        private static async Task<string> Registration(string user, string pass)
        {
            string token = "";
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var fullUrl = $"{url}/User/Registration?name={user}&pass={pass}";

                var response = await httpClient.PostAsync(fullUrl, null);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    token = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("-Регистрация успешно завершена");
                }
                else
                    Console.WriteLine("-Сервер отверг регистрацию");
            }
            Console.WriteLine("-Ваш токен: " + token);

            return token;
        }
        private static async void DeleteAccount(string token)
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var fullUrl = $"{url}/User/DeleteSelfAccount";
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);//по токену сервак 

                var response = await httpClient.PostAsync(fullUrl,null);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("-Аккаунт удален");
                }
                else
                    Console.WriteLine("Сервер ответил ошибкой");
            }
        }



        private static HttpClientHandler HandlerCustomCertCheck()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errorType) =>
                {
                    bool validCert = false;

                    if (errorType == SslPolicyErrors.RemoteCertificateChainErrors)
                        validCert = true;//игнорим ошибки цепочки сертификатов(т.к. может быть самоподписной)

                    if (validCert)//костыль, чтобы самим проверить сертификат сервака
                    {
                        string validPublicKey =
                            "MIIBCgKCAQEAoNb1K6RGwpivdQpzSyIRozPANl1hcUL" +
                            "Zqneh9ljARZ+I9uHwIAszqceE3UcRRhvQTxWBW4Z1Hh" +
                            "Cm/fI+BpzTC8XP4JkFv8P8gmEgFUet77wRNvVoFS12J" +
                            "Fl2Cu5JCETMM5V3mhGUOA44d5piAph6vEPcQIIocmrD" +
                            "mBXqHhnKVckqIp1+Y1biXcmbHDOZ6ZNGJs+aIC8TNKI" +
                            "nc9jn0kxPnfzgVPbmI86NB87xAG/PuEPvhYLiIO6rwa" +
                            "eGxKXDKuVk6qbGNdqilwvjlSONKAKFCMvYvSn7iNYSe" +
                            "bOzrwhRB5sCZkfE1ZuClGQMAp0qfW5NjkVwcqAEMHUH" +
                            "AEadgNw0KQIDAQAB";

                        var serverCert = new X509Certificate2(cert);
                        string serverPubKey = Convert.ToBase64String(serverCert.PublicKey.EncodedKeyValue.RawData);
                        if (serverPubKey != validPublicKey)
                            throw new Exception("Публичный ключ серверного сертификата невалиден!");
                    }

                    return validCert;
                }
            };
        }
#endregion





        private static async Task Start(HubConnection connection)
        {
            await connection.StartAsync();
            Console.WriteLine("-Соединение установлено.");
        }
        private static async Task Stop(HubConnection connection)
        {
            await connection.StopAsync();
            Console.WriteLine("-Соединение закрыто.");
        }


        private static async Task SendLoop(
            int fromUserID,
            HubConnection connection)
        {
            //Console.WriteLine("-Введите ID-Получателя:");
            //string toUserID = Console.ReadLine();
            //Console.WriteLine
            //// Здесь можно реализовать отправку сообщений
            //while (true)
            //{
            //    var text = PrintOutputMessage(toUserID);
            //    if (text.Equals("exit", StringComparison.OrdinalIgnoreCase))
            //    {
            //        break;
            //    }

            //    var message = new Message(-1, fromUserID, groupID, toUserID, MessageType.Text, text);
            //    await connection.InvokeAsync("SendMessageToClient", message);//отправка конкретному клиенту(через сервер)
            //}
        }
        private static string PrintOutputMessage(string toUser)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"To {toUser}:");
            var text = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;

            return text;
        }


        //варианты реализации
        private static void StartListenThread(string token)
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    Thread.Sleep(10_000);

                    using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
                    {
                        var fullUrl = $"{url}/Message/FindUnreaded";
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);//по токену сервак 

                        var response = await httpClient.GetAsync(fullUrl);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var unreadedDetected = await response.Content.ReadAsStringAsync();
                            bool detect = JsonSerializer.Deserialize<bool>(unreadedDetected);
                            if (detect)
                                Console.WriteLine("---есть новые сообщения---");
                        }
                        else
                            Console.WriteLine("Ошибка при прослушке сервер");
                    }
                }
            });
        }

    }
}
