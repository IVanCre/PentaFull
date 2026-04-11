using Client.Interfaces;
using Client.Platforms.Android.PushServices;
using Android.Content;
using AndroidApp=Android.App;
using Android.OS;
using Android.Provider;
using AndroidNet=Android.Net;

namespace Client.Platforms.Android
{
    internal class AndroidConfigurator : IPlatformConfigurator
    {
        public void FirstConfigurate()//настраиваем первичные разрешения
        {
            SetBatteryOptimizations();
            RegistrationDeviceForPush();
            RequestPermissions();
        }

        private void RegistrationDeviceForPush()
        {
            var sender = App.Services.GetRequiredService<DeviceTokenSender>();
            sender.SendTokenToServer();
        }

        private void SetBatteryOptimizations()//это чтобы фоновая активность не убивалась ОС
        {

            var intent = new Intent();
            var packageName = AndroidApp.Application.Context.PackageName;
            var pm = (PowerManager)AndroidApp.Application.Context.GetSystemService(Context.PowerService);
            if (!pm.IsIgnoringBatteryOptimizations(packageName))
            {
                intent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(AndroidNet.Uri.Parse("package:" + packageName));
                intent.SetFlags(ActivityFlags.NewTask);
                AndroidApp.Application.Context.StartActivity(intent);
            }
        }

        private async void RequestPermissions()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        }
    }
}
