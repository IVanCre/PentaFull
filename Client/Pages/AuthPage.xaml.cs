


using Penta_ClientLib.Interfaces;
using Client.Services;

namespace Client.Pages
{
	public partial class AuthPage : ContentPage
	{
		private IClientFacade _clientFacade;
        public AuthPage()
		{
			InitializeComponent();

			_clientFacade = App.Services.GetRequiredService<IClientFacade>();
			TryInicializeCredsFromSettings(App.Services.GetRequiredService<ISettingsHolder>());
        }


        private async void OnRegButtonClicked(object sender, EventArgs e)
		{
			if (UsernameEntry.Text != string.Empty && PasswordEntry.Text != string.Empty)
			{
				var result = await _clientFacade.Registration(UsernameEntry.Text, PasswordEntry.Text);
				if (result.Item1)
				{
					await NotificationService.ShowMessage("", "Регистрация успешно завершена", "ок");
                    await Shell.Current.GoToAsync("//ChatsPage");//перенаправление на страницу Чатов
                }
				else
					await NotificationService.ShowMessage("Внимание", $"Ошибка регистрации на сервере:{result.Item2.Message}", "ок");
			}
			else
				await NotificationService.ShowMessage("Внимание", "Введите логин и пароль", "ок");
		}

        private async void OnLoginButtonClicked(object sender, EventArgs e)
		{
			if (UsernameEntry.Text != string.Empty && PasswordEntry.Text != string.Empty)
			{
				var result = await _clientFacade.Login(UsernameEntry.Text, PasswordEntry.Text);
				if (result.Item1)
				{
					await NotificationService.ShowMessage("", "Вход успешно завершен", "ок");
                    await Shell.Current.GoToAsync("//ChatsPage");//перенаправление на страницу Чатов
                }
				else
					await NotificationService.ShowMessage("Внимание", $"Ошибка Входа на сервере:{result.Item2.Message}", "ок");
			}
			else
				await NotificationService.ShowMessage("Внимание", "Введите логин и пароль", "ок");
		}

		private async void TryInicializeCredsFromSettings(ISettingsHolder settings)
		{
            UsernameEntry.Text = await settings.GetValueByName<string>("userLogin");
            PasswordEntry.Text = await settings.GetValueByName<string>("userPassword");
        }
	}
}