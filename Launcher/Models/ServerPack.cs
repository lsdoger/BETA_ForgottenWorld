namespace Launcher.Models
{
    public class ServerPack
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string MinecraftVersion { get; set; } = "";
        public string Loader { get; set; } = "";
        public string LoaderVersion { get; set; } = "";

        public int Java { get; set; }
        public int Ram { get; set; }

        public string ServerIp { get; set; } = "";
        public int ServerPort { get; set; }
    }
}
