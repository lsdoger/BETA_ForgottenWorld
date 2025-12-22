using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
    public class LauncherController
    {
        private readonly MinecraftPath _minecraftPath;
        private readonly MinecraftLauncher _launcher;

        public LauncherController()
        {
            // Шлях до папки гри
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _minecraftPath = new MinecraftPath(Path.Combine(appData, ".ua_minecraft_launcher"));

            _launcher = new MinecraftLauncher(_minecraftPath);
        }

        public async Task LaunchGameAsync(string mcVersion, string type, string username, int ramMb, string serverIp, int serverPort)
        {
            var session = MSession.CreateOfflineSession(username);
            string versionToLaunch = mcVersion;

            try
            {
                // 1. Встановлення Fabric або Vanilla
                if (type.ToLower().Contains("fabric"))
                {
                    // Вручну завантажуємо JSON для Fabric
                    versionToLaunch = await InstallFabricManuallyAsync(mcVersion, "0.16.9");
                }
                else
                {
                    // Для ваніли просто вказуємо версію
                    await _launcher.InstallAsync(mcVersion);
                }

                // 2. Оновлюємо список версій
                var versions = await _launcher.GetAllVersionsAsync();

                // Знаходимо нашу версію в списку
                var bestVersion = versions.FirstOrDefault(v => v.Name == versionToLaunch);

                if (bestVersion == null)
                {
                    throw new Exception($"Версію {versionToLaunch} не знайдено (спробуйте перезапустити лаунчер).");
                }

                // --- ВИПРАВЛЕННЯ ТУТ ---
                // Ми передаємо bestVersion.Name (рядок), а не сам об'єкт
                await _launcher.InstallAsync(bestVersion.Name);

                // 3. Налаштування запуску
                var launchOption = new MLaunchOption
                {
                    MaximumRamMb = ramMb,
                    Session = session,
                    ServerIp = serverIp,
                    ServerPort = serverPort,
                    ScreenWidth = 1280,
                    ScreenHeight = 720
                    // Якщо потрібно: JavaPath = @"C:\Program Files\Java\jdk-21\bin\javaw.exe"
                };

                // 4. Створення та запуск процесу
                var process = await _launcher.CreateProcessAsync(versionToLaunch, launchOption);
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка запуску: {ex.Message}");
            }
        }

        private async Task<string> InstallFabricManuallyAsync(string mcVersion, string loaderVersion)
        {
            var versionId = $"fabric-loader-{loaderVersion}-{mcVersion}";
            var versionDir = Path.Combine(_minecraftPath.Versions, versionId);
            var jsonFilePath = Path.Combine(versionDir, $"{versionId}.json");

            if (File.Exists(jsonFilePath)) return versionId;

            try
            {
                var url = $"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}/{loaderVersion}/profile/json";
                using (var client = new HttpClient())
                {
                    var json = await client.GetStringAsync(url);
                    Directory.CreateDirectory(versionDir);
                    File.WriteAllText(jsonFilePath, json);
                }
                return versionId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Не вдалося завантажити профіль Fabric: {ex.Message}");
            }
        }

        public class ServerPack
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? MinecraftVersion { get; set; }
            public string? Loader { get; set; }
            public string? LoaderVersion { get; set; }
            public int Java { get; set; }
            public int Ram { get; set; }
            public string? ServerIp { get; set; }
            public int ServerPort { get; set; }
        }
    }
}