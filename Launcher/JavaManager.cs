using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Launcher
{
    public class JavaManager
    {
        public async Task<string> EnsureJava21Async(string basePath, Action<string>? onStatusChanged)
        {
            string javaRoot = Path.Combine(basePath, "runtime", "java21");
            string javaExe = Path.Combine(javaRoot, "bin", "javaw.exe");

            if (File.Exists(javaExe))
                return javaExe;

            Directory.CreateDirectory(basePath);
            onStatusChanged?.Invoke("Downloading Java 21...");

            using var http = new HttpClient();
            var zip = await http.GetByteArrayAsync(
                "https://api.adoptium.net/v3/binary/latest/21/ga/windows/x64/jre/hotspot/normal/eclipse"
            );

            string zipPath = Path.Combine(basePath, "java21.zip");
            await File.WriteAllBytesAsync(zipPath, zip);

            string extractDir = Path.Combine(basePath, "runtime", "java_tmp");
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);

            ZipFile.ExtractToDirectory(zipPath, extractDir, true);
            File.Delete(zipPath);

            var found = Directory
                .GetFiles(extractDir, "javaw.exe", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (found == null)
                throw new Exception("Java installation failed: javaw.exe not found");

            var home = Directory.GetParent(found)!.Parent!.FullName;

            if (Directory.Exists(javaRoot))
                Directory.Delete(javaRoot, true);

            Directory.Move(home, javaRoot);

            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);

            return javaExe;
        }
    }
}