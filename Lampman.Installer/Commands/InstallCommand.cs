namespace Lampman.Installer.Commands
{
    public static class InstallCommand
    {
        public static void Execute(string path, bool repair, bool addToPath, bool runAfter)
        {
            Install(path, repair, addToPath, runAfter);
        }

        public static void Install(string path, bool repair, bool addToPath, bool runAfter)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[Lampman] Installing into {path}...");
            Console.ResetColor();

            Directory.CreateDirectory(path);

            var cliReleasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "."));

            if (!Directory.Exists(cliReleasePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Lampman] Release folder not found: {cliReleasePath}");
                Console.ResetColor();
                return;
            }

            foreach (var file in Directory.GetFiles(cliReleasePath, "*", SearchOption.AllDirectories))
            {
                var target = file.Replace(cliReleasePath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);

                if (!File.Exists(target) || repair)
                {
                    File.Copy(file, target, true);
                }
            }

            // Generate stack.json
            var stackFile = Path.Combine(path, "stack.json");
            if (!File.Exists(stackFile) || repair)
            {
                File.WriteAllText(stackFile, @"{ ""services"": [] }");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[Lampman] stack.json created.");
                Console.ResetColor();
            }

            if (addToPath)
            {
                var envPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                if (envPath == null || !envPath.Contains(path))
                {
                    Environment.SetEnvironmentVariable(
                        "PATH", envPath + ";" + path,
                        EnvironmentVariableTarget.User);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Lampman] Added to PATH.");
                    Console.ResetColor();
                }
            }

            if (runAfter)
            {
                var exePath = Path.Combine(path, "lampman.exe");
                if (File.Exists(exePath))
                {
                    System.Diagnostics.Process.Start(exePath);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Lampman] Running Lampman...");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Lampman] Installation complete!");
            Console.ResetColor();
        }
    }
}
