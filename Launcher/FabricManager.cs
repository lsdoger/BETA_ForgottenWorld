using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Launcher
{
    public class FabricManager
    {
        public async Task<string> EnsureFabricViaJarAsync(string basePath, string mcVersion, string javaExe, Action<string>? onStatusChanged)
        {
            string installerDir = Path.Combine(basePath, "fabric-installer");
            Directory.CreateDirectory(installerDir);

            string installerJar = Path.Combine(installerDir, "fabric-installer.jar");

            if (!File.Exists(installerJar))
            {
                onStatusChanged?.Invoke("Downloading Fabric installer...");
                using var http = new HttpClient();
                var data = await http.GetByteArrayAsync(
                    "https://maven.fabricmc.net/net/fabricmc/fabric-installer/1.0.1/fabric-installer-1.0.1.jar"
                );
                await File.WriteAllBytesAsync(installerJar, data);
            }

            onStatusChanged?.Invoke("Installing Fabric...");

            var psi = new ProcessStartInfo
            {
                FileName = javaExe,
                Arguments = $"-jar \"{installerJar}\" client -mcversion {mcVersion} -dir \"{basePath}\" -noprofile",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var p = Process.Start(psi);
            p.WaitForExit();

            var version = Directory
                .GetDirectories(Path.Combine(basePath, "versions"), "fabric-loader-*")
                .OrderByDescending(d => d)
                .First();

            return Path.GetFileName(version);
        }
    }
}