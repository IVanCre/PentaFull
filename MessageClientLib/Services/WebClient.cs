using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using MessageClientLib.Interfaces;
using MessageLib;


namespace MessageClientLib.Services
{
    internal class WebClient : IWebClient
    {
        private HubConnection _messHabConnection;
        private string _serverUrl = "https://192.168.1.35:9093";
        private string _jwtToken= string.Empty;//здесь хранится токен от доступа от сервака
        public Func<Message, Task> MessageRecieveAsync { get; set; }//внешний делегат для обработки входящих сообщений ОТ сервера


        public async Task<bool> TryDeleteAccountAsync()
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var fullUrl = $"{_serverUrl}/User/DeleteSelfAccount";
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);//по токену сервак 

                var response = await httpClient.PostAsync(fullUrl, null);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("-Аккаунт удален");
                    return true;
                }
                else
                    Console.WriteLine("Сервер ответил ошибкой");
            }

            return false;
        }

        public async Task<bool> TryLoginAsync(string login, string pass)
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var fullUrl = $"{_serverUrl}/User/Login?name={login}&pass={pass}";

                var response = await httpClient.GetAsync(fullUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _jwtToken = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("-Вход завершен");
                    return true;
                }
                else
                    Console.WriteLine("-Сервер отверг вход");
            }

            return false;
        }

        public async Task<bool> TryRegisterAsync(string login, string pass)
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var fullUrl = $"{_serverUrl}/User/Registration?name={login}&pass={pass}";

                var response = await httpClient.PostAsync(fullUrl, null);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _jwtToken = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("-Регистрация успешно завершена");
                }
                else
                    Console.WriteLine("-Сервер отверг регистрацию");
            }

            return false;
        }




        public async Task<bool> ConnectToMessageHub()
        {
            try
            {
                _messHabConnection = new HubConnectionBuilder()
                    .WithUrl($"{_serverUrl}/exchanger", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_jwtToken);//передаем токен,чтоб пропустили на хаб
                        options.HttpMessageHandlerFactory = _ =>
                        {
                            return HandlerCustomCertCheck();
                        };
                    })
                    .Build();


                // Обработка входящих сообщений
                _messHabConnection.On<Message>("RecieveMessage", async (message) =>
                {
                    MessageRecieveAsync?.Invoke(message);//вызываем внешний делегат
                    await _messHabConnection.InvokeAsync("AcknowledgeReceived", message.ID);//подтверждение о получении
                });

                await _messHabConnection.StartAsync();

                return true;
            }
            catch (Exception ex) {  }

            return false;
        }
        private HttpClientHandler HandlerCustomCertCheck()
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

        public async void DisconnectFromMessageHub()
        {
            if(_messHabConnection!=null && _messHabConnection.State== HubConnectionState.Connected)
                await _messHabConnection.StopAsync();
        }

        public async Task<bool> TrySendMessageToHub(Message message)
        {
            try
            {
                if (_messHabConnection != null && _messHabConnection.State == HubConnectionState.Connected)
                {
                    await _messHabConnection.InvokeAsync("SendToServer", message);
                    return true;
                }
            }
            catch(Exception ex) { }

            return false;
        }
    }
}
