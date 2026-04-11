


using Penta_ClientLib.Interfaces;
using Client.Interfaces;


namespace Client.Pages
{
    public partial class AuthPage : ContentPage
	{
		private IClientFacade _clientFacade;
		private IDialogManager _notifier;
        private IPlatformConfigurator _platformConfigurator;

        public AuthPage()
		{
			InitializeComponent();

			_clientFacade = App.Services.GetRequiredService<IClientFacade>();
			_notifier = App.Services.GetRequiredService<IDialogManager>();
            _platformConfigurator= App.Services.GetRequiredService<IPlatformConfigurator>();
        }


        private async void OnRegButtonClicked(object sender, EventArgs e)
		{
			if (UsernameEntry.Text != string.Empty && PasswordEntry.Text != string.Empty)
			{
                LockUIForAwait();
				var result = await _clientFacade.RegistrationAsync(UsernameEntry.Text, PasswordEntry.Text);
                UnlockUI();
                if (result.Item1)
				{
                    _platformConfigurator?.FirstConfigurate();

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
                LockUIForAwait();
                var result = await _clientFacade.LoginAsync(UsernameEntry.Text, PasswordEntry.Text);
                UnlockUI();
                if (result.Item1)
                {
                    await Shell.Current.GoToAsync("//ChatsPage");//перенаправление на страницу Чатов
                }
                else
                    await _notifier.ShowMessage("Внимание", $"Ошибка входа:{result.Item2.Message}", "ок");
            }
        }

        private void LockUIForAwait()//чтобы во время ожидания юзер не шлепнул куда не надо))
        {
            LoaderSpin.IsRunning = true;
            UsernameEntry.IsEnabled = false;
            PasswordEntry.IsEnabled = false;
            RegButton.IsEnabled = false;
            LoginButton.IsEnabled = false;
        }
        private void UnlockUI()
        {
            LoaderSpin.IsRunning = false;
            UsernameEntry.IsEnabled = true;
            PasswordEntry.IsEnabled = true;
            RegButton.IsEnabled = true;
            LoginButton.IsEnabled = true;
        }
    }
}