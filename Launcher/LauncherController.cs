using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
    public class LauncherController
    {
        private readonly MinecraftPath _mcPath;
        private readonly MinecraftLauncher _launcher;
        private readonly JavaManager _javaManager;
        private readonly FabricManager _fabricManager;

        public event Action<string>? OnStatusChanged;

        public LauncherController()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _mcPath = new MinecraftPath(Path.Combine(appData, ".ua_minecraft_launcher"));
            _launcher = new MinecraftLauncher(_mcPath);

            // Ініціалізуємо наші нові класи-помічники
            _javaManager = new JavaManager();
            _fabricManager = new FabricManager();
        }

        public async Task LaunchGameAsync(
            string mcVersion,
            string username,
            int ramMb,
            string serverIp,
            int serverPort)
        {
            try
            {
                // 1. Java (викликаємо через менеджер)
                string javaPath = await _javaManager.EnsureJava21Async(_mcPath.BasePath, OnStatusChanged);

                // 2. Session
                var session = MSession.CreateOfflineSession(username);

                // 3. Vanilla
                OnStatusChanged?.Invoke("Installing Minecraft...");
                await _launcher.InstallAsync(mcVersion);

                // 4. Fabric (викликаємо через менеджер)
                string fabricVersion = await _fabricManager.EnsureFabricViaJarAsync(_mcPath.BasePath, mcVersion, javaPath, OnStatusChanged);

                // 5. Launch
                OnStatusChanged?.Invoke("Launching Minecraft...");

                var opt = new MLaunchOption
                {
                    Session = session,
                    MaximumRamMb = ramMb,
                    JavaPath = javaPath,
                    ServerIp = serverIp,
                    ServerPort = serverPort
                };

                var process = await _launcher.CreateProcessAsync(fabricVersion, opt);
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Launch error");
            }
        }
    }
}