using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

using Message_Server.Interfaces;
using Message_Server.Services;
using Message_Server.Services.SignalR;
using Message_Server.Services.Repositories;
using Message_Server.Services.MessagesProcessors;
using Message_Server.Utilits;
using Message_Server.Services.Loggers;

namespace Message_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                ConfigValidator.Check(builder.Configuration);//провер€ем один раз перед запуском

                AddServicesImplementations(builder.Services);
                SetSecurity(builder);

                var app = builder.Build();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseHttpsRedirection();
                app.MapHub<ChatHub>("/chat");

                StartServices(app.Services);

                app.Run();
            }
            catch (Exception ex)//чтобы контроллировать сам пуск
            {
                OverheadLogger.LogError(ex.Message);
            }
        }

        private static void AddServicesImplementations(IServiceCollection services)
        {
            services.AddSingleton<ILogWriter, LogWriter>();
            services.AddSingleton<ILogReader, LogReader>();
            services.AddSingleton<LogFolderCleaner>();

            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IMessageRepository, MessageRepository>();
            services.AddSingleton<IConnectionsRepository, ConnectionsRepository>();
            services.AddSingleton<IGroupRepository, GroupRepository>();

            services.AddSingleton<IMessageSaver, MessageSaver>();
            services.AddSingleton<ITokenManager, TokenManager>();
            services.AddSingleton<MessagesDBCleaner>();
            services.AddSingleton<ICopyUserDetector, CopyUserDetector>();
            services.AddSingleton<IMessageProcessor, MessageProcessor>();

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>//настраиваем возможность ввода токена на странице сваггера
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme//тут настраиваем описание ui-формочки
                {
                    Description =
                        "ƒл€ корректной авторизации на этой странице, получите свой токен(черз запрос Login) и \r\n\r\n" +
                        "вставьте его в поле ввода, предварительно добавив в начале 'Bearer '. \r\n\r\n" +
                        "ѕример: Bearer 1sdfkhgsdkfl\r\n\r\n",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()//добавл€ем возможность автоматической авторизации
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

            services.AddSignalR();
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

                                ValidateIssuer = true,//проверка издател€
                                ValidIssuer = builder.Configuration["Jwt:Issuer"],//издатель токена
                                ValidateAudience = true,//проверка потребител€
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

        private static void StartServices(IServiceProvider sprovider)
        {
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
