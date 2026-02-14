
using Client.Models;
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;
using MessageLib;
using Client.Interfaces;
using System.Globalization;

namespace Client.Pages
{
	public class LabelAlignConverter : IValueConverter//выравнивает сообщение по левому\правому краю в зависимости от отправителя
	{
		public object Convert(object value, Type t, object p, CultureInfo c)
		{
            if (value is string a)
            {
                if (string.IsNullOrEmpty(a))
                    return TextAlignment.Start;// Нет отправителя, значит сообщение от пользователя — слева
                else 
                    return TextAlignment.End;// Входящее сообщение от другого пользователя — справа
            }
            
            return TextAlignment.Start;// По умолчанию, если что-то не распознано
        }

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}



	public partial class ActiveChatPage : ContentPage
	{
		public ObservableCollection<MessageInfo> MessageList { get; set; }
		public string ChatName { get; private set; }//если приватный -имя контакта
		private int _recieverUserID = -1;//используется,только если это приватный чат
		private int _chatID;
		private int _currentUserID;//идентификатор юзера
		private IClientFacade _clientFacade;
		private IUINotificator _notifier;
		private IContactHolder _contactHolder;
		private ISettingsHolder _settingsHolder;
		private const int messageLoadedNum = 100;//количество последних сообщений для загрузки при старте


		public ActiveChatPage(string name, int chatID)
		{
			InitializeComponent();

			_clientFacade = App.Services.GetRequiredService<IClientFacade>();
			_notifier = App.Services.GetRequiredService<IUINotificator>();
			_contactHolder = App.Services.GetRequiredService<IContactHolder>();
			_settingsHolder =App.Services.GetRequiredService<ISettingsHolder>();
            
            _clientFacade.MessageAddedToChat += TryAddIncomingMessageToChat;
			MessageList = new();
			ChatName = name;
			_chatID = chatID;

			LoadLastMessages();
			BindingContext = this;
		}
		private async void LoadLastMessages()
		{
			var finded = await _clientFacade.GetMessagesByChat(_chatID, messageLoadedNum);
			_currentUserID =await _settingsHolder.GetValueByName<int>("userID");
			string from=string.Empty;
			foreach (var mess in finded)
			{
				if (mess.FromID != _currentUserID)//чтобы распознавать где входящие сообщения
					from = mess.FromID.ToString();
				else
					from= string.Empty;


                switch (mess.Type)
				{
					case MessageType.Text: MessageList.Add(new MessageInfo() {From=from, Text = mess.GetDataLikeString() }); break;

					case MessageType.Picture: MessageList.Add(new MessageInfo() { From =from, Text = "Unsupported Message Type" }); break;//на будущее задел
					case MessageType.Voice: MessageList.Add(new MessageInfo() { From =from, Text = "Unsupported Message Type" }); break;
				}
			}
		}

		private void TryAddIncomingMessageToChat(int chatID, Message msg)
		{
			if (chatID == _chatID)//групповой чат
			{
				MessageList.Add(
					new MessageInfo()
					{
						From = msg.FromID.ToString(),
						Text = msg.GetDataLikeString()
					});
			}
			else
			{
				if (chatID == -1 && _recieverUserID == msg.FromID)//отправитель-это тот, кому мы пишем в этом чате
					MessageList.Add(
						new MessageInfo()
						{
							From = msg.FromID.ToString(),
							Text = msg.GetDataLikeString()
						});
			}
		}


		private async void OnSendMessage(object sender, EventArgs e)        //Пока работаем только с текстом!
		{
			var input = MessageText.Text;
			MessageList.Add(new MessageInfo() { Text = input });

			Tuple<bool, Exception> result = default;
			if (_chatID > 0)
			{
				result = await _clientFacade.SendMessageToChat(_chatID, MessageType.Text, MessageUtils.TextToBytes(input));
			}
			else
			{
				if (_recieverUserID == -1)
					_recieverUserID = await _contactHolder.GetUserIDByName(ChatName);//тянем идентификатор юзера

				result = await _clientFacade.SendMessageToUser(_chatID,_recieverUserID, MessageType.Text, MessageUtils.TextToBytes(input));
			}

			if (!result.Item1)
				await _notifier.ShowMessage("Ошибка", $"Сообщение НЕ ОТПРАВЛЕНО:{result.Item2.Message}", "ок");

			MessageText.Text = "";
		}
	}
}