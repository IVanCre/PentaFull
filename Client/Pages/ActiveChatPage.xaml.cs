
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
		public bool CanShowConnectionState = false;//используется в приватном чате

		private ChatInfo _chatInfo;
		private int _recieverUserID = -1;//используется,только если это приватный чат
		private int? _currentUserID;//идентификатор юзера
		private Dictionary<int, string> _cashedNames = new();//чтоб не бегать в БД на каждый пук
		private IClientFacade _clientFacade;
		private ISettingsProvider _settingsHolder;
        private bool _isHistoryLoading = false;//идет ли подгрузка старых сообщений
        private int _messToLoadCount = 5;//количество сообщений для загрузки\подгрузки


		public ActiveChatPage(ChatInfo chat)
		{
			InitializeComponent();

			MessageList = new();
			_chatInfo = chat;
            this.Unloaded += OnPageUnloaded;
        }

		protected override async void OnAppearing()//при ЛЮБОМ отображении страницы(старт\после сна)
		{
			base.OnAppearing();

			if (_clientFacade == null)//первичное отображение страницы
			{
				_clientFacade = App.Services.GetRequiredService<IClientFacade>();
				_clientFacade.MessageAddedToChat += TryAddIncomingMessageToChat;
				_clientFacade.ChatDeleted += ChatDeleted;
				_clientFacade.MessageSendedOnServer += MakrMessageLikeSended;
				_clientFacade.RecieverConnectedChanged += RecieverConnectionChanged;//чтобы понимать когда получатель приватного чата в сети
				_settingsHolder = App.Services.GetRequiredService<ISettingsProvider>();

				ConfigurateByType();
				LoadLastMessagesAsync();

				if (IsThisChatIsPrivate())
				{
					ConnectionStateText.Text = "отключен";
                    CanShowConnectionState = true;

					_recieverUserID = await _clientFacade.GetRecieverIDFromChatAsync(_chatInfo.ChatName);
					_clientFacade.StartObserveUserConnection(_chatInfo.ID, _recieverUserID);//просим присылать изменения подключения  получателя
				}
			}
            BindingContext = this;
		}

        private void OnPageUnloaded(object sender, EventArgs e)
        {
			if (_chatInfo.HaveUnreadedMessages)
			{
				_chatInfo.HaveUnreadedMessages = false;
				_clientFacade.MarkChatLikeReaded(_chatInfo.ID);
			}

            this.Unloaded -= OnPageUnloaded;
            _clientFacade.MessageAddedToChat -= TryAddIncomingMessageToChat;
            _clientFacade.ChatDeleted -= ChatDeleted;
            _clientFacade.MessageSendedOnServer -= MakrMessageLikeSended;
            _clientFacade.RecieverConnectedChanged -= RecieverConnectionChanged;

            _clientFacade.EndObserveUserConnection(_chatInfo.ID, _recieverUserID);
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
				HistoryRefresher.IsRefreshing = true;

				_ = Task.Factory.StartNew(async () =>
			   {
				   if(_currentUserID==null)
                       _currentUserID = await _settingsHolder.GetUserID();

                   DateTimeOffset timestampToStart = (MessageList.Count > 0) ? MessageList[0].TimestampData : DateTimeOffset.UtcNow;
				   var buffer = new List<MessageInfo>();
				   var findedMessages = await _clientFacade.GetOldMessagesByChatAsync(_chatInfo.ID, _messToLoadCount, timestampToStart);
				   if (findedMessages.Count() > 0)
				   {
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
								   ID = mess.ID,
								   IsSended= mess.IsSendedToServer
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



        private async void TryAddIncomingMessageToChat(int chatID, Message msg)
		{
			if (chatID == _chatInfo.ID ||//групповой чат
				chatID == -1 && _recieverUserID == msg.FromID)//чат не указан, значит смотрим отправителя
			{
				var sender = await GetUserName(msg.FromID);
                var added =AddNewMesageToList(Direction.Input, sender, msg.GetDataLikeString(), msg.UtcTimestamp);
				added.IsSended = true;
            }
		}
		private async void OnSendMessage(object sender, EventArgs e) 
		{
			var input = MessageText.Text;
			var senderName =await GetUserName(_currentUserID.Value);//чтобы имена определялись в единой точке
			var msg =AddNewMesageToList(Direction.Output,senderName, input, DateTime.Now);

			MessageText.Text = "";

			if (IsThisChatIsPrivate())//значит приватный чат
			{
				await _clientFacade.SendMessageToUserAsync(
					_chatInfo.ID,
					_recieverUserID,
					MessageType.Text,
					MessageUtils.TextToBytes(input),
					msg.ID);
			}
			else//значит групповой чат
			{
				await _clientFacade.SendMessageToGroupChatAsync(
					_chatInfo.ID,
					MessageType.Text,
					MessageUtils.TextToBytes(input),
					msg.ID);
			}
        }

        private MessageInfo AddNewMesageToList(Direction msgDirection,string sender, string text, DateTimeOffset timestamp)
		{
			var msg = new MessageInfo() {
				ID = Guid.NewGuid(),
				Type = msgDirection,
				SenderName = sender,
				Text = text,
				IsSended = msgDirection == Direction.Input,//входящие всегда доставлены))))
				TimestampData = timestamp,
			};

            MessageList.Add(msg);

			AutoScrollToBottom();

			return msg;
        }
		private void AutoScrollToBottom()//вызываем прокрутку к новому сообщению
		{
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var lastItem = MessageList.LastOrDefault();
                if (lastItem != null)
                    MessagesListView.ScrollTo(lastItem, position: ScrollToPosition.End, animate: true);
            });
        }

		private void MakrMessageLikeSended(Guid messageID)
		{
			var finded = MessageList.FirstOrDefault(x => x.ID == messageID);
			if(finded!=null)
				finded.IsSended = true;
		}

		private void RecieverConnectionChanged(int chatID, int userID, bool state)
		{
			if (chatID == _chatInfo.ID && _recieverUserID == userID)//значит это наш получатель
				MainThread.BeginInvokeOnMainThread(() =>
				{
					ConnectionStateText.Text = state == true ? "подключен" : "отключен";
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

		private bool IsThisChatIsPrivate()
		{
			return _chatInfo.ID < 0;
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