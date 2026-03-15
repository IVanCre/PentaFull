using Android.Content;
using Client.Interfaces;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Interfaces;
using System.Reflection;


namespace Client.Platforms.Android.UpdateServices
{
    internal class AndroidUpdateManager(IWebClient client) : IUpdateManager
    {
        private IWebClient _client = client;

        public async Task TryUpdateClientAsync(ClientType type)
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var fileName = await _client.GetNewestClientFilaName(version, type);

                if (!string.IsNullOrEmpty(fileName))
                {
                    var loadedFile = await LoadNewVersion(fileName, type);
                    if (!string.IsNullOrEmpty(loadedFile))
                        InstallNewVersion(loadedFile);
                }
            }
            catch (Exception ex) { }
        }


        private async Task<string> LoadNewVersion(string fileName, ClientType type)
        {
            string localPath = string.Empty;
            CleanCache();

            var data = await _client.LoadClientFileAsync(fileName, type);
            if (data != null)
            {
                localPath = Path.Combine(FileSystem.CacheDirectory, "update_penta_client.apk");
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await data.CopyToAsync(fileStream);
            }

            return localPath;
        }
        private void CleanCache()
        {
            var cachePath = FileSystem.CacheDirectory;
            var apkFiles = Directory.GetFiles(cachePath, "*.apk");
            foreach (var file in apkFiles)
                File.Delete(file);
        }

        private void InstallNewVersion(string filePath)
        {
            Intent intent = new Intent(Intent.ActionView);

            var context = Platform.CurrentActivity ?? Platform.AppContext;
            var file = new Java.IO.File(filePath);
            var apkUri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, $"{context.PackageName}.fileprovider", file);            // Формируем URI через FileProvider

            intent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
            intent.AddFlags(ActivityFlags.NewTask);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission); // Даем права на чтение файла системе

            context.StartActivity(intent);
        }


    }
}
