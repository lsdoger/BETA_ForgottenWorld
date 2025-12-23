using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
    public class LauncherController
    {
        private readonly MinecraftPath _mcPath;
        private readonly MinecraftLauncher _launcher;

        public event Action<string>? OnStatusChanged;

        public LauncherController()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _mcPath = new MinecraftPath(Path.Combine(appData, ".ua_minecraft_launcher"));
            _launcher = new MinecraftLauncher(_mcPath);
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
                // 1. Java
                string javaPath = await EnsureJava21Async();

                // 2. Session
                var session = MSession.CreateOfflineSession(username);

                // 3. Vanilla
                OnStatusChanged?.Invoke("Installing Minecraft...");
                await _launcher.InstallAsync(mcVersion);

                // 4. Fabric
                string fabricVersion = await EnsureFabricViaJarAsync(mcVersion, javaPath);

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
            private async Task<string> EnsureFabricViaJarAsync(string mcVersion, string javaExe)
        {
            string installerDir = Path.Combine(_mcPath.BasePath, "fabric-installer");
            Directory.CreateDirectory(installerDir);

            string installerJar = Path.Combine(installerDir, "fabric-installer.jar");

            if (!File.Exists(installerJar))
            {
                OnStatusChanged?.Invoke("Downloading Fabric installer...");
                using var http = new HttpClient();
                var data = await http.GetByteArrayAsync(
                    "https://maven.fabricmc.net/net/fabricmc/fabric-installer/1.0.1/fabric-installer-1.0.1.jar"
                );
                await File.WriteAllBytesAsync(installerJar, data);
            }

            OnStatusChanged?.Invoke("Installing Fabric...");

            var psi = new ProcessStartInfo
            {
                FileName = javaExe,
                Arguments = $"-jar \"{installerJar}\" client -mcversion {mcVersion} -dir \"{_mcPath.BasePath}\" -noprofile",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var p = Process.Start(psi);
            p.WaitForExit();

            // знайти створену fabric-версію
            var version = Directory
                .GetDirectories(Path.Combine(_mcPath.BasePath, "versions"), "fabric-loader-*")
                .OrderByDescending(d => d)
                .First();

            return Path.GetFileName(version);
        }

        private async Task<string> EnsureJava21Async()
        {
            string baseDir = _mcPath.BasePath; // ВАЖЛИВО
            string javaRoot = Path.Combine(baseDir, "runtime", "java21");
            string javaExe = Path.Combine(javaRoot, "bin", "javaw.exe");

            if (File.Exists(javaExe))
                return javaExe;

            Directory.CreateDirectory(baseDir);

            OnStatusChanged?.Invoke("Downloading Java 21...");

            using var http = new HttpClient();
            var zip = await http.GetByteArrayAsync(
                "https://api.adoptium.net/v3/binary/latest/21/ga/windows/x64/jre/hotspot/normal/eclipse"
            );

            string zipPath = Path.Combine(baseDir, "java21.zip");
            await File.WriteAllBytesAsync(zipPath, zip);

            string extractDir = Path.Combine(baseDir, "runtime", "java_tmp");
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);

            ZipFile.ExtractToDirectory(zipPath, extractDir, true);
            File.Delete(zipPath);

            // 🔍 шукаємо javaw.exe де б він не був
            var found = Directory
                .GetFiles(extractDir, "javaw.exe", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (found == null)
                throw new Exception("Java installation failed: javaw.exe not found");

            var home = Directory.GetParent(found)!.Parent!.FullName;

            if (Directory.Exists(javaRoot))
                Directory.Delete(javaRoot, true);

            Directory.Move(home, javaRoot);

            // прибираємо тимчасову папку
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);

            return javaExe;
        }
    }
}
