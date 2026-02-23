


using Penta_ClientLib.Interfaces;
using Client.Interfaces;
using Client.Platforms.Android;

namespace Client.Pages
{
	public partial class AuthPage : ContentPage
	{
		private IClientFacade _clientFacade;
		private IUINotificator _notifier;

        public AuthPage()
		{
			InitializeComponent();

			_clientFacade = App.Services.GetRequiredService<IClientFacade>();
			_notifier = App.Services.GetRequiredService<IUINotificator>();
        }


        private async void OnRegButtonClicked(object sender, EventArgs e)
		{
			if (UsernameEntry.Text != string.Empty && PasswordEntry.Text != string.Empty)
			{
				var result = await _clientFacade.RegistrationAsync(UsernameEntry.Text, PasswordEntry.Text);
                if (result.Item1)
				{
					SetBatteryOptimizations();
                    RegistrationDevice();

                    await Shell.Current.GoToAsync("//ChatsPage");//перенаправление на страницу Чатов
                }
				else
					await _notifier.ShowMessage("Внимание", $"Ошибка регистрации на сервере:{result.Item2.Message}", "ок");
			}
			else
				await _notifier.ShowMessage("Внимание", "Введите логин и пароль", "ок");
		}



        private void RegistrationDevice()
        {
#if ANDROID
            var sender =App.Services.GetRequiredService<DeviceTokenSender>();
            sender.SendTokenToServer();
#endif
        }
		private void SetBatteryOptimizations()//это чтобы фоновая активность не убивалась ОС
		{
#if ANDROID
            var intent = new Android.Content.Intent();
            var packageName = Android.App.Application.Context.PackageName;
            var pm = (Android.OS.PowerManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.PowerService);
            if (!pm.IsIgnoringBatteryOptimizations(packageName))
            {
                intent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
            }
#endif
        }
    }
}