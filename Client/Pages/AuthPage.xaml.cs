


using Penta_ClientLib.Interfaces;
using Client.Interfaces;

namespace Client.Pages
{
	public partial class AuthPage : ContentPage
	{
		private IClientFacade _clientFacade;
		private IUINotificator _notifier;
		private INewMessageCheckerManager _newMessageListener;

        public AuthPage()
		{
			InitializeComponent();

			_clientFacade = App.Services.GetRequiredService<IClientFacade>();
			_notifier = App.Services.GetRequiredService<IUINotificator>();
			_newMessageListener = App.Services.GetRequiredService<INewMessageCheckerManager>();

            TryInicializeCredsFromSettings(App.Services.GetRequiredService<ISettingsHolder>());

			if(string.IsNullOrEmpty(UsernameEntry.Text))//значит есть данные с прошлой регистрации
				LoginButton.IsVisible = false;
			else
				RegButton.IsVisible = false;
        }


        private async void OnRegButtonClicked(object sender, EventArgs e)
		{
			if (UsernameEntry.Text != string.Empty && PasswordEntry.Text != string.Empty)
			{
				var result = await _clientFacade.Registration(UsernameEntry.Text, PasswordEntry.Text);
                if (result.Item1)
				{
					_newMessageListener.StartService();
                    await Shell.Current.GoToAsync("//ChatsPage");//перенаправление на страницу Чатов
                }
				else
					await _notifier.ShowMessage("Внимание", $"Ошибка регистрации на сервере:{result.Item2.Message}", "ок");
			}
			else
				await _notifier.ShowMessage("Внимание", "Введите логин и пароль", "ок");
		}

        private async void OnLoginButtonClicked(object sender, EventArgs e)
		{
			if (UsernameEntry.Text != string.Empty && PasswordEntry.Text != string.Empty)
			{
				var result = await _clientFacade.Login(UsernameEntry.Text, PasswordEntry.Text);
				if (result.Item1)
				{
					_newMessageListener.StartService();
                    await Shell.Current.GoToAsync("//ChatsPage");//перенаправление на страницу Чатов
                }
				else
					await _notifier.ShowMessage("Внимание", $"Ошибка Входа на сервере:{result.Item2.Message}", "ок");
			}
			else
				await _notifier.ShowMessage("Внимание", "Введите логин и пароль", "ок");
		}

		private async void TryInicializeCredsFromSettings(ISettingsHolder settings)
		{
            UsernameEntry.Text = await settings.GetValueByName<string>("userLogin");
            PasswordEntry.Text = await settings.GetValueByName<string>("userPassword");
        }


		private void OffServiceClicked(object sender, EventArgs e)
		{
            _newMessageListener.StopService();
        }

    }
}