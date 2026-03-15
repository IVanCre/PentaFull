using Android.App;
using Android.Content;


namespace Client.Platforms.Android.UpdateServices
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionMyPackageReplaced })]
    public class UpdateReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // Проверяем, что обновилось именно наше приложение
            if (intent.Action == Intent.ActionMyPackageReplaced)
            {
                // Получаем Intent для запуска главной Activity
                Intent launchIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
                if (launchIntent != null)
                {
                    launchIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                    context.StartActivity(launchIntent);//инициируем запуск экземпляра приложения(которое обновилось)
                }
            }
        }
    }
}
