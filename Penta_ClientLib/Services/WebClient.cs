using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography.X509Certificates;
using Penta_ClientLib.Interfaces;
using MessageLib;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using Penta_ClientLib.DataStructs;
using Microsoft.Extensions.DependencyInjection;



namespace Penta_ClientLib.Services
{
    internal abstract class BaseClient
    {
        protected ISettingsProvider _settingsHolder;
        protected int _waitRequestSeconds = 90;        
        protected string _serverUrl = "https://192.168.1.35:9093";

        protected BaseClient(ISettingsProvider settings)
        {
            if(settings==null)
                throw new ArgumentNullException(nameof(settings));
            _settingsHolder = settings;
        }

        protected HttpClientHandler HandlerCustomCertCheck()
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
        protected async Task<bool> TryRefreshToken(HttpStatusCode response)
        {
            bool result = false;
            switch (response)
            {
                case HttpStatusCode.Unauthorized://возможно, токен истек
                    {
                        if (await TryRefreshJwtToken())
                            result= true;
                    }
                    break;
                case HttpStatusCode.OK:
                    result = true;
                    break;
            }
            return result;
        }
        private async Task<bool> TryRefreshJwtToken()
        {
            var _refreshJwtToken = await _settingsHolder.GetRefreshToken();//нет токена-нет запроса))
            if (!string.IsNullOrEmpty(_refreshJwtToken))
            {
                using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);
                    var fullUrl = $"{_serverUrl}/User/RefreshToken";
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _refreshJwtToken);//по токену сервак 

                    var response = await httpClient.GetAsync(fullUrl);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var serialized = await response.Content.ReadAsStringAsync();
                        string[] tokenPack = JsonSerializer.Deserialize<string[]>(serialized);
                        await SaveTokenPack(tokenPack);

                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Сервер ответил ошибкой");
                        return false;
                    }
                }
            }
            else
                return false;
        }
        protected async Task SaveTokenPack(string[] tokenPack)
        {
            await _settingsHolder.SetAccessToken(tokenPack[0]);
            await _settingsHolder.SetRefreshToken(tokenPack[1]);
        }
    }


    // НЕ ПЕРЕХВАТЫВАТЬ ОШИБКИ ТУТ!  ПУСТЬ ВСПЛЫВАЮТ ВВЕРХ ДЛЯ ПОВЫШЕНИЯ ИНФОРМАТИВНОСТИ
    internal class WebClient :BaseClient, IWebClient
    {
        private int _pingIntervalSeconds = 5;
        private HubConnection _messHabConnection;
        public event MessageSended MessageSended;
        public event ConnectionStateChanged ConnectionStateChanged;
        public event MessageRecieved RecievedMessage;//внешний делегат для обработки входящих сообщений ОТ сервера

        public WebClient(ISettingsProvider settings) : base(settings) { }


        #region withoutJwt
        public async Task<Tuple<int, Exception>> TryEnterAsync(string login, string pass, bool isRegistration)
        {
            int userID = -1;

            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);
                string fullUrl = string.Empty;
                if(isRegistration)
                    fullUrl = $"{_serverUrl}/User/Registration?name={login}&pass={pass}";
                else
                    fullUrl = $"{_serverUrl}/User/Login?name={login}&pass={pass}";

                var response = await httpClient.PostAsync(fullUrl, null);
                var serialized = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        {
                            string[] tokenPack = JsonSerializer.Deserialize<string[]>(serialized);
                            await SaveTokenPack(tokenPack);

                            userID = GetUserID(await _settingsHolder.GetAccessToken());
                            await ConnectToMessageHub();

                            return Tuple.Create<int, Exception>(userID, null);
                        }
                    case HttpStatusCode.Conflict:
                    case HttpStatusCode.BadRequest:
                        {
                            var doc = JsonDocument.Parse(serialized);
                            string message = doc.RootElement.GetProperty("message").GetString();
                            return Tuple.Create<int, Exception>(userID, new Exception(message));
                        }
                }
            }
            return Tuple.Create<int, Exception>(userID, null);
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


        public async Task<string> GetNewestClientFilaName(string currentClientVersion, ClientType type)//просто запрашиваем имя файла самого свежего билда клиента
        {
            string urlWithNewVersion = string.Empty;

            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);

                var fullUrl = $"{_serverUrl}/Updates/GetNewestClientFileName?currentClientVersion={currentClientVersion}&type={type}";

                var response = await httpClient.GetAsync(fullUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    urlWithNewVersion = await response.Content.ReadAsStringAsync();
                }
            }
            return urlWithNewVersion;
        }
        public async Task<HttpContent> LoadClientFileAsync(string fileName, ClientType type)
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);

                var fullUrl = $"{_serverUrl}/Updates/LoadFile?fileName={fileName}&type={type}";

                var response = await httpClient.GetAsync(fullUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                    return response.Content;
                else
                    return null;
            }
        }
        #endregion


        #region withJwtOnly
        public async Task<bool> SendDeviceToken(string tokenDevice)
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var accessToken = await _settingsHolder.GetAccessToken();
                httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);//по токену сервак 

                var fullUrl = $"{_serverUrl}/PushRegistrator/SetDevice?tokenDevice={tokenDevice}";

                var response = await httpClient.PostAsync(fullUrl, null);
                if (await TryRefreshToken(response.StatusCode))
                {
                    return true;
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);//тут уже новый токен 
                    response = await httpClient.PostAsync(fullUrl, null);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }

        public async Task<bool> TryDeleteAccountAsync()
        {
            using (var httpClient = new HttpClient(HandlerCustomCertCheck()))
            {
                var accessToken = await _settingsHolder.GetAccessToken();
                httpClient.Timeout = TimeSpan.FromSeconds(_waitRequestSeconds);
                var fullUrl = $"{_serverUrl}/User/DeleteSelfAccount";
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);//по токену сервак 

                var response = await httpClient.PostAsync(fullUrl, null);
                if (await TryRefreshToken(response.StatusCode))
                {
                    Console.WriteLine("-Аккаунт удален");
                    return true;
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);//тут уже новый токен 
                    response = await httpClient.PostAsync(fullUrl, null);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }


        public async Task<bool> SendMessage(Message message)
        {
            if (await ConnectToMessageHub())
            {
                if (_messHabConnection != null && _messHabConnection.State == HubConnectionState.Connected)
                {
                    await _messHabConnection.InvokeAsync("SendToServer", message);
                    MessageSended?.Invoke(message.ID);//чтобы БД смогла отметить отправленные
                    return true;
                }
            }
            //ошибки не ловим -позволяем всплыть вверх по стеку вызовов

            return false;
        }

        public bool IsConnected()
        {
            if(_messHabConnection!=null)
                return _messHabConnection.State == HubConnectionState.Connected;
            else
                return false;
        }
        public async Task<bool> ConnectToMessageHub()
        {

            if (_messHabConnection == null)
            {
                 _messHabConnection = new HubConnectionBuilder()
                    .WithUrl($"{_serverUrl}/exchanger", options =>
                    {
                        options.AccessTokenProvider = async () =>// Динамический провайдер: вызывается ПЕРЕД каждым (пере)подключением
                        {
                            var token = await _settingsHolder.GetAccessToken();
                            var secondsToDie = _settingsHolder.GetLifetimeSecondsLeft(token);//сколько секунд до истечения осталось
                            if (secondsToDie.TotalSeconds < 30)
                            {
                                var refreshed = await TryRefreshToken(HttpStatusCode.Unauthorized);
                                if (refreshed)
                                    token = await _settingsHolder.GetAccessToken();
                            }
                            return token;
                        };
                        options.HttpMessageHandlerFactory = _ => HandlerCustomCertCheck();
                        
                    })
                    .WithAutomaticReconnect(new InfiniteReconnectPolicy())
                    .AddMessagePackProtocol()
                    .Build();

                _messHabConnection.KeepAliveInterval=TimeSpan.FromSeconds(_pingIntervalSeconds);


                _messHabConnection.Closed += async (ex) =>
                {
                    ConnectionStateChanged?.Invoke(false);//отключились
                    await InicializeConnect();// Сюда попадаем, если переподключение не удалось (например, нет сети)
                };
                _messHabConnection.Reconnecting += async (ex) =>//идет переподключение
                {
                    ConnectionStateChanged?.Invoke(false);
                    await Task.CompletedTask;
                };
                _messHabConnection.Reconnected += async (ex) =>//переподключились
                {
                    ConnectionStateChanged?.Invoke(true);
                    await Task.CompletedTask;
                };
            }

            await InicializeConnect();
            return _messHabConnection.State == HubConnectionState.Connected;
        }
        private class InfiniteReconnectPolicy : IRetryPolicy//кастомный 
        {
            public TimeSpan? NextRetryDelay(RetryContext retryContext)
            {
                return TimeSpan.FromSeconds(5);
            }
        }
        private async Task InicializeConnect()
        {
            if (_messHabConnection.State == HubConnectionState.Disconnected)
            {
                _messHabConnection.On<Message>("RecieveMessage", async (message) =>
                {
                    RecievedMessage?.Invoke(message);//вызываем внешний делегат
                    await _messHabConnection.InvokeAsync("AcknowledgeReceived", message.ID);//подтверждение о получении для сервака
                });
                await _messHabConnection.StartAsync();

                if (_messHabConnection.State == HubConnectionState.Connected)
                    ConnectionStateChanged?.Invoke(true);
            }
        }

        #endregion

        private async void DisconnectFromMessageHub()
        {
            if (_messHabConnection != null)
            {
                await _messHabConnection.StopAsync();
                await _messHabConnection.DisposeAsync();
                _messHabConnection = null;
                ConnectionStateChanged?.Invoke(false);
            }
        }
        public void Dispose()
        {
            DisconnectFromMessageHub();
        }

    }
}
