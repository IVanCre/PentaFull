using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography.X509Certificates;
using Penta_ClientLib.Interfaces;
using MessageLib;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;



namespace Penta_ClientLib.Services
{
    public class WebClient : IWebClient
    {
        private int _waitRequestSeconds = 90;
        private HubConnection _messHabConnection;
        private string _serverUrl = "https://192.168.1.35:9093";
        private string _jwtToken= string.Empty;//здесь хранится токен от доступа от сервака

        public event MessageRecieved RecievedMessage;//внешний делегат для обработки входящих сообщений ОТ сервера


        public async Task<bool> TryDeleteAccountAsync()
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                httpClient.Timeout= TimeSpan.FromSeconds(_waitRequestSeconds);
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
                httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);
                var fullUrl = $"{_serverUrl}/User/Login?name={login}&pass={pass}";

                var response = await httpClient.GetAsync(fullUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _jwtToken = await response.Content.ReadAsStringAsync();

                    await ConnectToMessageHub();
                    Console.WriteLine("-Вход завершен");
                    return true;
                }
                else
                    Console.WriteLine("-Сервер отверг вход");
            }

            return false;
        }
        public async Task<int> TryRegisterAsync(string login, string pass)
        {
            int userID = -1;
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);
                var fullUrl = $"{_serverUrl}/User/Registration?name={login}&pass={pass}";

                var response = await httpClient.PostAsync(fullUrl, null);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _jwtToken = await response.Content.ReadAsStringAsync();
                    userID = GetUserID(_jwtToken);
                    await ConnectToMessageHub();
                    Console.WriteLine("-Регистрация успешно завершена");
                }
                else
                    Console.WriteLine("-Сервер отверг регистрацию");
            }

            return userID;
        }
        private int GetUserID(string token)
        {
            int userID = -1;
            var handler = new JwtSecurityTokenHandler();
            var jwtTokenObj = handler.ReadJwtToken(token);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(jwtTokenObj.Claims));
            foreach (var claim in claimsPrincipal.Claims)
            {
                if(claim.Type=="userID")
                {
                    userID= int.Parse(claim.Value);
                    break;
                }
            }

            return userID;
        }




        public async Task<bool> ConnectToMessageHub()
        {
            try
            {
                if (_messHabConnection == null)
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


                    // Обработка входящих сообщений с сервера
                    _messHabConnection.On<Message>("RecieveMessage", async (message) =>
                    {
                        RecievedMessage?.Invoke(message);//вызываем внешний делегат
                        await _messHabConnection.InvokeAsync("AcknowledgeReceived", message.ID);//подтверждение о получении
                    });

                    await _messHabConnection.StartAsync();
                }
                else
                {
                    if (_messHabConnection.State == HubConnectionState.Disconnected)//соединение может закрыться, если кто-то долго не отвечает
                       await _messHabConnection.StartAsync();
                }

                return true;
            }
            catch (Exception ex) 
            {  
                return false;
            }


        }
        private HttpClientHandler HandlerCustomCertCheck()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errorType) =>
                {
                    //игнорим любые ошибки -проверяем сам факт данных сертификата
                    bool serverCertIsValid = false;
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
                    else
                        serverCertIsValid = true;

                    return serverCertIsValid;
                }
            };
        }

        private async void DisconnectFromMessageHub()
        {
            if (_messHabConnection != null)
            {
                await _messHabConnection.StopAsync();
                await _messHabConnection.DisposeAsync();
                _messHabConnection = null;
            }
        }

        public async Task<bool> SendMessage(Message message)
        {
            try
            {
                await ConnectToMessageHub();

                if (_messHabConnection != null && _messHabConnection.State == HubConnectionState.Connected)
                {
                    await _messHabConnection.InvokeAsync("SendToServer", message);
                    return true;
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        public void Dispose()
        {
            DisconnectFromMessageHub();
            _jwtToken = string.Empty;
        }
    }
}
