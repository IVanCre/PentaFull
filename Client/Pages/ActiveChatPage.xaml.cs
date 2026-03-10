
using Client.Models;
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;
using MessageLib;
using Client.Interfaces;
using Penta_ClientLib.Services;


namespace Client.Pages
{

	public partial class ActiveChatPage : ContentPage
	{
		public ObservableCollection<MessageInfo> MessageList { get; set; }
		public string ChatName { get; private set; }//если приватный -имя контакта
		private int _recieverUserID = -1;//используется,только если это приватный чат
		private int _currentChatID;
		private int _currentUserID;//идентификатор юзера
		private IClientFacade _clientFacade;
		private IUINotificator _notifier;
		private ISettingsProvider _settingsHolder;
		private const int messageLoadedNum = 100;//количество последних сообщений для загрузки при старте


		public ActiveChatPage(string name, int chatID)
		{
			InitializeComponent();

			MessageList = new();
			ChatName = name;
			_currentChatID = chatID;
			_clientFacade = App.Services.GetRequiredService<IClientFacade>();
			_notifier = App.Services.GetRequiredService<IUINotificator>();
			_settingsHolder =App.Services.GetRequiredService<ISettingsProvider>();

            _clientFacade.MessageAddedToChat += TryAddIncomingMessageToChat;
			_clientFacade.UserAdded += UserAdded;
			_clientFacade.UserRemoved += UserRemoved;
			_clientFacade.ChatDeleted += ChatDeleted;
			TryInicializeAddToGroupButton();
			LoadLastMessages();
			BindingContext = this;
		}
		private async void TryInicializeAddToGroupButton()//если ты админ группы - у тебя есть кнопки добавить\удалить юзера из группы
		{
			if (_recieverUserID == -1)//это групповой чат
			{
				bool userIsGroupAdmin = await _clientFacade.AmCreatedGroupChat(_currentChatID);
				if (userIsGroupAdmin)
				{
					var item = new ToolbarItem();
					item.Text = "Members";
					item.Clicked += (o, e) =>
					{
						MembersManager.Show(_currentChatID);//вызываем наше всплывающее окно
                    };
					this.ToolbarItems.Add(item);
				}
			}
		}

        private async void LoadLastMessages()
		{
			var finded = await _clientFacade.GetMessagesByChatAsync(_currentChatID, messageLoadedNum);
			_currentUserID =await _settingsHolder.GetUserID();
			TextAlignment alignType;
			foreach (var mess in finded)
			{
				if (mess.FromID == _currentUserID)
					alignType = TextAlignment.Start;//собственные сообщения слева
				else
					alignType = TextAlignment.End;//все остальное -справа


                switch (mess.Type)
				{
					case MessageType.Text: MessageList.Add(new MessageInfo() {Type=alignType, Text = mess.GetDataLikeString() }); break;

					case MessageType.Picture: MessageList.Add(new MessageInfo() { Type =alignType, Text = "Unsupported Message Type" }); break;//на будущее задел
					case MessageType.Voice: MessageList.Add(new MessageInfo() { Type =alignType, Text = "Unsupported Message Type" }); break;
				}
			}
		}

        private void TryAddIncomingMessageToChat(int chatID, Message msg)
		{
			if (chatID == _currentChatID)//групповой чат
			{
				MessageList.Add(
					new MessageInfo()
					{
                        Type = TextAlignment.End,
						Text = msg.GetDataLikeString()
					});
			}
			else
			{
				if (chatID == -1 && _recieverUserID == msg.FromID)//отправитель-это тот, кому мы пишем в этом чате
					MessageList.Add(
						new MessageInfo()
						{
                            Type = TextAlignment.End,
							Text = msg.GetDataLikeString()
						});
			}
		}
		private async void OnSendMessage(object sender, EventArgs e)        //Пока работаем только с текстом!
		{
			var input = MessageText.Text;
			MessageList.Add(new MessageInfo() {Type=TextAlignment.Start, Text = input });
			MessageText.Text = "";

			Tuple<bool, Exception> result = default;
			if (_currentChatID > 0)
			{
				result = await _clientFacade.SendMessageToGroupChatAsync(_currentChatID, MessageType.Text, MessageUtils.TextToBytes(input));
			}
			else
			{
				_recieverUserID =await _clientFacade.GetRecieverIDFromChatAsync(ChatName);
                result = await _clientFacade.SendMessageToUserAsync(_currentChatID,_recieverUserID, MessageType.Text, MessageUtils.TextToBytes(input));
			}

			if (!result.Item1)
				await _notifier.ShowMessage("Ошибка", $"Сообщение НЕ ОТПРАВЛЕНО:{result.Item2?.Message}", "ок");
		}


        #region несохраняемые уведомления
        private void UserAdded(int chatID, int userID)
		{
			string text=string.Empty;
			if (userID == _currentUserID)
				text = "ВЫ ДОБАВЛЕНЫ В ЧАТ";
			else
				text = $"В ЧАТ ДОБАВЛЕН: {ContactConverter.ConvertUserIDToContactID(userID)}";

            MessageList.Add(new MessageInfo() { Type = TextAlignment.Center, Text = text });
        }
		private void UserRemoved(int chatID, int userID)
		{
            string text = string.Empty;
            if (userID == _currentUserID)
                text = "ВЫ УДАЛЕНЫ ИЗ ЧАТА";
            else
                text = $"ИЗ ЧАТА УДАЛЕН: {ContactConverter.ConvertUserIDToContactID(userID)}";

            MessageList.Add(new MessageInfo() { Type = TextAlignment.Center, Text = text });
        }
		private void ChatDeleted(int chatID, string chatName)
        {
			if (_currentChatID == chatID)
			{
				string text = "ЧАТ УДАЛЕН ХОЗЯИНОМ";
				MessageList.Add(new MessageInfo() { Type = TextAlignment.Center, Text = text });
			}
        }
        #endregion
    }
}