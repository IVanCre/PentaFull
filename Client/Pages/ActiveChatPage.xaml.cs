
using Client.Models;
using Client.Interfaces;
using Client.UIElements;

using MessageLib;

using Penta_ClientLib.Interfaces;
using Penta_ClientLib.Services;
using Penta_ClientLib.DataStructs;



namespace Client.Pages
{

    public partial class ActiveChatPage : ContentPage
	{
		public CustomObservableCollection<MessageInfo> MessageList { get; set; }
		public string ChatName => _chatInfo.ChatName;

		private ChatInfo _chatInfo;
		private int _recieverUserID = -1;//используется,только если это приватный чат
		private int _currentUserID;//идентификатор юзера
		private Dictionary<int, string> _cashedNames = new();//чтоб не бегать в БД на каждый пук
		private IClientFacade _clientFacade;
		private IDialogManager _notifier;
		private ISettingsProvider _settingsHolder;
        private bool _isHistoryLoading = false;//идет ли подгрузка старых сообщений
        private int _messToLoadCount = 5;//количество сообщений для загрузки\подгрузки


		public ActiveChatPage(ChatInfo chat)
		{
			InitializeComponent();

			MessageList = new();
			_chatInfo = chat;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			_clientFacade = App.Services.GetRequiredService<IClientFacade>();
			_notifier = App.Services.GetRequiredService<IDialogManager>();
			_settingsHolder = App.Services.GetRequiredService<ISettingsProvider>();

			_clientFacade.MessageAddedToChat += TryAddIncomingMessageToChat;
			_clientFacade.ChatDeleted += ChatDeleted;
			ConfigurateByType();

            if (BindingContext == null)
                BindingContext = this;

            LoadLastMessagesAsync();//чтобы сразу запустилась подгрузка
		}

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _clientFacade.MessageAddedToChat -= TryAddIncomingMessageToChat;
            _clientFacade.ChatDeleted -= ChatDeleted;
			if(_chatInfo.ChatType== ChatType.Group)
			{
                _clientFacade.UserAdded -= UserAdded;
                _clientFacade.UserRemoved -= UserRemoved;
            }
        }

        #region первичная инициализация
        private void ConfigurateByType()
		{
			switch (_chatInfo.ChatType)
			{
				case ChatType.Group:
					{
						TryInicializeAddToGroupButton();
						_clientFacade.UserAdded += UserAdded;
						_clientFacade.UserRemoved += UserRemoved;
					}
					break;
				case ChatType.ReadOnly:
					{
						MessageText.IsEnabled = false;
						MessageText.IsVisible = false;

						SendButton.IsEnabled = false;
						SendButton.IsVisible = false;
					}
					break;
			}
		}
		private async void TryInicializeAddToGroupButton()//если ты админ группы - у тебя есть кнопки добавить\удалить юзера из группы
		{
			bool userIsGroupAdmin = await _clientFacade.AmCreatedGroupChat(_chatInfo.ID);
			if (userIsGroupAdmin)
			{
				var item = new ToolbarItem();
				item.Text = "Members";
				item.Clicked += (o, e) =>
				{
					MembersManager.Show(_chatInfo.ID);//вызываем наше всплывающее окно
				};
				this.ToolbarItems.Add(item);
			}
		}
		private void LoadLastMessagesAsync()//от этой метки более старые просим
		{
			if (!_isHistoryLoading)
			{
				_isHistoryLoading = true;
				DateTimeOffset timestampToStart = (MessageList.Count > 0) ? MessageList[0].TimestampData : DateTimeOffset.UtcNow;
				HistoryRefresher.IsRefreshing = true;

				_ = Task.Factory.StartNew(async () =>
			   {
				   var buffer = new List<MessageInfo>();
				   var findedMessages = await _clientFacade.GetOldMessagesByChatAsync(_chatInfo.ID, _messToLoadCount, timestampToStart);
				   if (findedMessages.Count() > 0)
				   {
					   _currentUserID = await _settingsHolder.GetUserID();
					   foreach (var mess in findedMessages)
					   {
						   var sender = await GetUserName(mess.FromID);
						   buffer.Add(
							   new MessageInfo
							   {
								   Type = (mess.FromID == _currentUserID) ? Direction.Output : Direction.Input,
								   SenderName = sender,
								   Text = mess.GetDataLikeString(),
								   TimestampData = mess.UtcTimestamp,
							   });
					   }
				   }

                   buffer.Reverse();
				   MainThread.BeginInvokeOnMainThread(() =>
				   {
					   MessageList.InsertRange(0, buffer);
					   HistoryRefresher.IsRefreshing = false;
					   _isHistoryLoading = false;
                   });
			   });
			}
		}
        #endregion

        private void OnHistoryLoading(object sender, EventArgs e)=>LoadLastMessagesAsync();


#region изменение коллекции
        private async void TryAddIncomingMessageToChat(int chatID, Message msg)
		{
			if (chatID == _chatInfo.ID ||//групповой чат
				chatID == -1 && _recieverUserID == msg.FromID)//чат не указан, значит смотрим отправителя
			{
				var sender = await GetUserName(msg.FromID);
                AddNewMesageToList(Direction.Input, sender, msg.GetDataLikeString(), msg.UtcTimestamp);
			}
		}
		private async void OnSendMessage(object sender, EventArgs e)        //Пока работаем только с текстом!
		{
			var input = MessageText.Text;
			var senderName =await GetUserName(_currentUserID);//чтобы имена определялись в единой точке
			AddNewMesageToList(Direction.Output,senderName, input, DateTime.Now);
			MessageText.Text = "";

			Tuple<bool, Exception> result = default;
			if (_chatInfo.ID > 0)//значит групповой чат
			{
				result = await _clientFacade.SendMessageToGroupChatAsync(_chatInfo.ID, MessageType.Text, MessageUtils.TextToBytes(input));
			}
			else//значит приватный чат
			{
				_recieverUserID = await _clientFacade.GetRecieverIDFromChatAsync(_chatInfo.ChatName);
				result = await _clientFacade.SendMessageToUserAsync(_chatInfo.ID, _recieverUserID, MessageType.Text, MessageUtils.TextToBytes(input));
			}

			if (!result.Item1)
				await _notifier.ShowMessage("Ошибка", $"Сообщение НЕ ОТПРАВЛЕНО:{result.Item2?.Message}", "ок");
		}
#endregion

        private void AddNewMesageToList(Direction msgDirection,string sender, string text, DateTimeOffset timestamp)
		{
            MessageList.Add(
				new MessageInfo(){ 
					Type = msgDirection,
					SenderName = sender,
					Text = text,
					TimestampData=timestamp,
				});
        }
		private async Task<string> GetUserName(int userID)
		{
			if (userID == _currentUserID)
				return "я";

			if (_cashedNames.TryGetValue(userID, out string findedName))
				return findedName;
			else
			{
				string name =await _clientFacade.FindUserPseudonimeByID(userID);
				_cashedNames.Add(userID, name);
				return name;
			}	
        }


#region несохраняемые уведомления
        private void UserAdded(int chatID, int userID)
		{
			string text=string.Empty;
			if (userID == _currentUserID)
				text = "ВЫ ДОБАВЛЕНЫ В ЧАТ";
			else
				text = $"В ЧАТ ДОБАВЛЕН: {ContactConverter.ConvertUserIDToContactID(userID)}";

            AddNewMesageToList(Direction.Input,"система", text,DateTime.Now);
        }
		private void UserRemoved(int chatID, int userID)
		{
            string text = string.Empty;
            if (userID == _currentUserID)
                text = "ВЫ УДАЛЕНЫ ИЗ ЧАТА";
            else
                text = $"ИЗ ЧАТА УДАЛЕН: {ContactConverter.ConvertUserIDToContactID(userID)}";

            AddNewMesageToList(Direction.Input, "система", text, DateTime.Now);
        }
		private void ChatDeleted(int chatID, string chatName)
        {
			if (_chatInfo.ID == chatID)
			{
				string text = "ЧАТ УДАЛЕН ХОЗЯИНОМ";
                AddNewMesageToList(Direction.Input, "система", text, DateTime.Now);
            }
        }
        #endregion
    }
}