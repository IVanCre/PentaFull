using Penta_Server.Interfaces;

namespace Penta_Server.Services
{
    public class ClientBuildsObserver(IConfiguration config) : IClientFileObserver
    {
        private IConfiguration _config = config;
        private int _versionSize = 4;//число цифр в версии major.minor.build.revision

        public Task<FileStream> ReadFileAsync(string fileName, ClientType type)
        {
            return Task<FileStream>.Factory.StartNew(() =>
            {
                string buildFolderType = string.Empty;
                switch (type)
                {
                    case ClientType.Android: buildFolderType = "Android"; break;
                    case ClientType.Windows: buildFolderType = "Windows"; break;
                }
                string filePath = Path.Combine(_config["ClientBuildsFolder"], buildFolderType, fileName);
                if (File.Exists(filePath))
                    return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                else
                    return null;
            });
        }


        public async Task<string> GetNewClientVersionFileNameAsync(string oldClientVersion, ClientType type)
        {
            var parsedVersion = ParseVersion(oldClientVersion);
            if (parsedVersion != null)
            {
                var finded = await GetAllVersionsAsync(type);
                return GetNewestClientFileName(finded, parsedVersion);
            }
            else
                return string.Empty;
        }
        public async Task<string> GetNewClientVersionFileNameAsync(ClientType type)
        {
            var finded = await GetAllVersionsAsync(type);
            if (finded != null && finded.Count > 0)
                return finded[0].Item1;
            else
                return string.Empty;
        }

        private int[] ParseVersion(string inputVersion)
        {
            int[] parsedVersion = null;
            string[] parsed = inputVersion.Split('.');
            try
            {
                parsedVersion = new int[_versionSize];
                for (int i = 0; i < parsed.Length; i++)
                {
                    if(i<=_versionSize)
                        parsedVersion[i] = int.Parse(parsed[i]);
                }
            }
            catch (Exception ex)//если строка невалидная
            {
                parsedVersion = null;
            }

            return parsedVersion;
        }
        private async Task<List<Tuple<string, int[]>>> GetAllVersionsAsync(ClientType type)
        {
            return await Task<List<Tuple<string, int[]>>>.Factory.StartNew(() =>
            {
                List<Tuple<string, int[]>> allVersions = new();
                string fileFolder = _config["ClientBuildsFolder"];
                if (!string.IsNullOrEmpty(fileFolder))
                {
                    switch (type)
                    {
                        case ClientType.Android: fileFolder = Path.Combine(fileFolder, "Android"); break;
                        case ClientType.Windows: fileFolder = Path.Combine(fileFolder, "Windows"); break;
                    }

                    string[] files = Directory.GetFiles(fileFolder);
                    foreach (string fileName in files)
                    {
                        var splitted = Path.GetFileNameWithoutExtension(fileName).Split('_');
                        var parsedVersion = ParseVersion(splitted[1]);
                        allVersions.Add(Tuple.Create(fileName, parsedVersion));
                    }
                }
                return allVersions//сортируем по убыванию версий
                                .OrderByDescending(arr => arr.Item2[0])
                                .ThenByDescending(arr => arr.Item2[1])
                                .ThenByDescending(arr => arr.Item2[2])
                                .ToList();
            });
        }
        private string GetNewestClientFileName(List<Tuple<string, int[]>> allVersions, int[] oldClientVersion)
        {
            //т.е. смотрим более новый билд.
            var finded =allVersions.FirstOrDefault(x => x.Item2[0] >  oldClientVersion[0] ||
                                                   x.Item2[0] == oldClientVersion[0] && x.Item2[1] > oldClientVersion[1] ||
                                                   x.Item2[0] == oldClientVersion[0] && x.Item2[1] == oldClientVersion[1] && x.Item2[2] > oldClientVersion[2]);//версия билда не проверяется
            if (finded != null)
                return Path.GetFileName(finded.Item1);//возвращаем только имя файла и ничего более
            else
                return string.Empty;
        }


    }
}
