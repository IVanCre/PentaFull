using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

using Penta_Server.Interfaces;
using Penta_Server.Services;
using Penta_Server.Services.SignalR;
using Penta_Server.Services.Repositories;
using Penta_Server.Services.MessagesProcessors;
using Penta_Server.Utilits;
using Penta_Server.Services.Loggers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;


namespace Penta_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = args,
                    ContentRootPath = AppContext.BaseDirectory
                });
                SetConfigFileByEnv(builder);
                ConfigValidator.Check(builder.Configuration);//проверяем один раз перед запуском

                ConfigureKestrel(builder);
                builder.Host.UseWindowsService();
                AddServicesImplementations(builder.Services);
                SetSecurity(builder);


                var app = builder.Build();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseHttpsRedirection();
                app.MapHub<MessageHub>("/exchanger");

                StartServices(app.Services);

                app.Run();
            }
            catch (Exception ex)//чтобы контроллировать сам пуск
            {
                OverheadLogger.LogError(ex.Message);
                throw ex;
            }
        }

        private static void SetConfigFileByEnv(WebApplicationBuilder builder)
        {
#if DEBUG
            string fileName = "config.Debug.json";
#else
            string fileName = "config.Release.json";
#endif
            builder.Configuration.AddJsonFile(fileName, optional: false);
        }

        private static void AddServicesImplementations(IServiceCollection services)
        {
            services.AddSingleton<ILogWriter, LogWriter>();
            services.AddSingleton<ILogReader, LogReader>();
            services.AddSingleton<LogFolderCleaner>();

            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IMessageRepository, MessageRepository>();
            services.AddSingleton<IGroupChatRepository, GroupRepository>();
            services.AddSingleton<MessagesDBCleaner>();
            services.AddSingleton<IConnectionsRepository, ConnectionsRepository>();
            services.AddSingleton<IDeviceTokenRepository, DeviceTokenRepository>();

            services.AddSingleton<IMessageSaver, MessageSaver>();
            services.AddSingleton<ITokenManager, TokenManager>();
            services.AddSingleton<IMessageProcessor, MessageProcessor>();
            services.AddSingleton<IClientNotifier, ClientNotifier>();
            services.AddSingleton<IPushManager, PushManager>();
            services.AddSingleton<IClientFileObserver, ClientFileObserver>();
            services.AddSingleton<IUsersConnectionObserver, UsersConnectionObserver>();

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>//настраиваем возможность ввода токена на странице сваггера
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme//тут настраиваем описание ui-формочки
                {
                    Description =
                        "Для корректной авторизации на этой странице,\r\n\r\n" +
                        "1.получите свой токен(черз запрос Login) и \r\n\r\n" +
                        "вставьте его(первый из двух) в поле ввода, предварительно добавив в начале 'Bearer '. \r\n\r\n" +
                        "Пример: Bearer 1sdfkhgsdkfl\r\n\r\n",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()//добавляем возможность автоматической авторизации
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            },
                            Scheme="oauth2",
                            Name="Bearer",
                            In=ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
        }

        private static void SetSecurity(IHostApplicationBuilder builder)
        {
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new()
                            {
                                RoleClaimType = ClaimTypes.Role,

                                ValidateIssuer = true,//проверка издателя
                                ValidIssuer = builder.Configuration["Jwt:Issuer"],//издатель токена
                                ValidateAudience = true,//проверка потребителя
                                ValidAudience = builder.Configuration["Jwt:Audience"],//потребитель токена
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,//проверка ключа
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))//сам ключ безопасности
                            };

                            options.Events = new JwtBearerEvents
                            {
                                OnAuthenticationFailed = context =>
                                {
                                    OverheadLogger.LogError($"Authentication failed: {context.Exception.Message}");
                                    return Task.CompletedTask;
                                },
                            };
                        });
        }

        //используем свой сертификат и работаем с ним локально
        //без поиска в хранилище хоста,танцев и бубнов
        private static void ConfigureKestrel(WebApplicationBuilder builder)
        {
            builder.WebHost.ConfigureKestrel((context, serverOptions) =>
            {
                // 1. Позволяем Kestrel автоматически загрузить все эндпоинты из секции "Kestrel"
                serverOptions.Configure(context.Configuration.GetSection("Kestrel"));

                // 2. Для ВСЕХ HTTPS эндпоинтов (включая наш "Https") 
                // переопределяем способ загрузки сертификата, добавляя MachineKeySet
                serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                {
                    var certSection = context.Configuration.GetSection("Kestrel:Endpoints:Https:Certificate");
                    var path = certSection.GetValue<string>("Path");
                    var password = certSection.GetValue<string>("Password");

                    if (!string.IsNullOrEmpty(path))
                    {
                        var certPath = Path.Combine(AppContext.BaseDirectory, path);

                        // Загружаем сертификат правильно для Windows-службы
                        httpsOptions.ServerCertificate = new X509Certificate2(
                            certPath,
                            password,
                            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
                    }
                });
            });
        }
        private static void StartServices(IServiceProvider sprovider)
        {
            var logger = sprovider.GetRequiredService<ILogWriter>();
            logger.SaveInfo("\n");
            logger.SaveInfo($"<---start-new-work---v.{Assembly.GetExecutingAssembly().GetName().Version.ToString()}--->");//чтобы по логам можно было понять когда стартовал\схлопнулся
#if DEBUG
            logger.SaveInfo("--DEBUG--");
#endif

            var config = sprovider.GetRequiredService<IConfiguration>();
            using (DB db= new DB(config["WorkDB:ConnString"]))
            {
               db.Database.Migrate();
            }

            sprovider.GetRequiredService<MessagesDBCleaner>()?.Start();
            sprovider.GetRequiredService<LogFolderCleaner>()?.Start();
        }
    }
}
