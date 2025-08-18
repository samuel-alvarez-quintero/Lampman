namespace Lampman.Installer.Commands
{
    public static class UninstallCommand
    {
        public static void Execute(string path, bool removeFromPath)
        {
            Uninstall(path, removeFromPath);
        }

        public static void Uninstall(string path, bool removeFromPath)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Lampman] Uninstalling from {path}...");
            Console.ResetColor();

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Lampman] Installation folder removed.");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Lampman] Failed to remove folder: {ex.Message}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("[Lampman] No installation found at this path.");
            }

            if (removeFromPath)
            {
                var envPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                if (envPath != null && envPath.Contains(path))
                {
                    var newPath = string.Join(";", envPath.Split(';').Where(p => !p.Equals(path, StringComparison.OrdinalIgnoreCase)));
                    Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Lampman] Removed from PATH.");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Lampman] Uninstall complete!");
            Console.ResetColor();
        }
    }
}
