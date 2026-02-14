
using AndroidAppLib = Android.App;//иначе в коде путаница по неймингам идет
using AndroidX.Work;//понадобилось отдельный пакет ставить, который обновил кучу транзитивных пакетов. Пришлось перейти на net9 из-за конфликтов
using Client.Interfaces;

namespace Client.Platforms.Android
{
    internal class MessageCheckerService : INewMessageCheckerManager
    {
        private string _serviceName = "PentaMessageChecker";
        public const int IntervalToCallInMinutes = 15;

        public void StartService()
        {
            var workRequest = PeriodicWorkRequest.Builder
                .From<MessageCheckWorker>(TimeSpan.FromMinutes(IntervalToCallInMinutes))
                .Build();

            WorkManager.GetInstance(AndroidAppLib.Application.Context)
                .EnqueueUniquePeriodicWork(_serviceName,
                                           ExistingPeriodicWorkPolicy.Update,
                                           workRequest);

        }

        public void StopService()
        {
            var result = WorkManager.GetInstance(AndroidAppLib.Application.Context)
                 .CancelUniqueWork(_serviceName);

            var _notifier = new NotificationHelper(AndroidAppLib.Application.Context);
            _notifier.ShowNotification($"PentaService", "Сервис остановлен");
        }
    }
}
