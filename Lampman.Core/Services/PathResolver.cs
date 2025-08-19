namespace Lampman.Core
{
    public static class PathResolver
    {
        public static readonly string RootDir;
        public static readonly bool IsDev;
        public static readonly List<string> DefaultRegistrySource;

        static PathResolver()
        {
            // 1. Check for install.json in production path
            string defaultInstallRoot = @"C:\Lampman";
            string installFile = Path.Combine(defaultInstallRoot, "install.json");

            if (File.Exists(installFile))
            {
                // Production mode
                RootDir = defaultInstallRoot;
                IsDev = false;
            }
            else
            {
                // Dev mode → relative to solution/project
                RootDir = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, @"..\..\..\.."));
                IsDev = true;
            }

            DefaultRegistrySource = new List<string>
            {
                "https://raw.githubusercontent.com/samuel-alvarez-quintero/Lampman/refs/tags/v1.1.0/registry/MainOriginServices.json"
            };
        }

        public static string RegistryFile => Path.Combine(RootDir, "registry.json");
        public static string ServicesFile => Path.Combine(RootDir, "services.json");
        public static string StackFile => Path.Combine(RootDir, "stack.json");

        public static string ServicesInstallDir => Path.Combine(RootDir, "services");

        public static string ServicePath(string serviceName, string version) =>
            Path.Combine(ServicesInstallDir, serviceName, version);

        public static string BinDir => Path.Combine(RootDir, "bin");
    }
}
