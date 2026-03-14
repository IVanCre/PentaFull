using Client.Interfaces;
using Penta_ClientLib.Interfaces;



namespace Client.Pages
{
	public partial class ChatCreatorPage : ContentPage
	{
        private IClientFacade _clientFacade;
        private IUINotificator _notifier;
        private List<ContactInfo> _contactsList { get; set; } = new();


        public ChatCreatorPage()
		{
			InitializeComponent();
            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            _notifier = App.Services.GetRequiredService<IUINotificator>();
        }
        private async void OnFocused(object sender, FocusEventArgs e)
        {
            _contactsList = await _clientFacade.GetAllContactsAsync();//т.к. чаты могут быть созданы в длругом месте тоже
            ContactList.ItemsSource = _contactsList;
            DropdownBorder.IsVisible = _contactsList.Any();
        }

        private async void OnUnfocused(object sender, FocusEventArgs e)
        {
            // Задержка 200мс нужна, чтобы клик по элементу списка успел обработаться 
            // до того, как список скроется
            await Task.Delay(200);
            DropdownBorder.IsVisible = false;
        }
        private void OnContactSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ContactInfo selected)
            {
                ChatNameEntry.Text = selected.UserContactID;
                DropdownBorder.IsVisible = false;

                // Сбрасываем выделение, чтобы можно было выбрать тот же элемент снова
                ContactList.SelectedItem = null;
            }
        }


        private async void OnCreateChat(object sender, EventArgs e)
        {
            var chatName = ChatNameEntry.Text;
            if (!string.IsNullOrEmpty(chatName))
            {
                if (IsGroupBox.IsChecked)
                {
                    var sended = await _clientFacade.CreateGroupChatAsync(chatName);
                    if (sended.Item1)
                        await Navigation.PopAsync();
                    else
                        await _notifier.ShowMessage("Ошибка", $"Ошибка при отправке запроса: {sended.Item2?.Message}", "ОК");
                }
                else
                {
                    var sended = await _clientFacade.CreatePrivateChatAsync(chatName);
                    if (sended!=0)
                        await Navigation.PopAsync();
                    else
                        await _notifier.ShowMessage("Ошибка", $"Приватный чат НЕ создан", "ОК");
                }
            }
        }
    }
}