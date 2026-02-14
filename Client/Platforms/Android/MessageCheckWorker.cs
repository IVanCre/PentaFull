
using AndroidAppLib = Android.App;//иначе в коде путаница по неймингам идет

using Android.Content;
using AndroidX.Work;
using Penta_ClientLib.Interfaces;//понадобилось отдельный пакет ставить, который обновил кучу транзитивных пакетов. Пришлось перейти на net9 из-за конфликтов


namespace Client.Platforms.Android
{
    internal class MessageCheckWorker : Worker//фоновая задача которая проверяет новые входящие сообщения
    {
        private NotificationHelper _notifier;
        private IWebClient _messChecker;
        private int _repeats;
        private int _intervalToCheck = 30_000;

        public MessageCheckWorker(Context context, WorkerParameters workerParams) : base(context, workerParams) 
        {
            _messChecker = App.Services.GetRequiredService<IWebClient>();
            _repeats = (MessageCheckerService.IntervalToCallInMinutes * 60_000) / 30_000;//15минут делим на 30 секунд
        }


        public override Result DoWork()
        {
            CheckNewMessages();
            return Result.InvokeSuccess();//информируем, что задача завершилась
        }

        private async void CheckNewMessages()
        {
            int num = 0;
            while(num<_repeats)//просто заглушка
            {
                if(_notifier==null)
                    _notifier = new NotificationHelper(AndroidAppLib.Application.Context);

                if(await _messChecker.CheckNewMessages())
                    _notifier.ShowNotification($"Новые сообщения", "Нажмите, чтобы посмотреть");

                num++;
                Thread.Sleep(_intervalToCheck);
            }

        }
    }
}
