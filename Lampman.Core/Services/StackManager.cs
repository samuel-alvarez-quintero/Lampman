using Lampman.Core.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Lampman.Core.Services
{
    public class StackManager
    {
        // ANSI escape codes for colors
        const string ANSI_RED = "\u001B[31m";
        const string ANSI_GREEN = "\u001B[32m";
        const string ANSI_BlUE = "\e[0;34m";
        const string ANSI_YELLOW = "\e[0;33m";
        const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

        private readonly StackConfig _config;

        public StackManager()
        {
            var StackconfigFile = PathResolver.StackFile;

            if (!File.Exists(StackconfigFile))
            {
                Console.WriteLine($"{ANSI_BlUE}[INFO] No config file found. Creating default stack.json...{ANSI_RESET}");

                var defaultConfig = new StackConfig
                {
                    Services = new List<ServiceProcess>([])
                };

                var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(StackconfigFile, json);
            }

            var fileContent = File.ReadAllText(StackconfigFile);
            _config = JsonSerializer.Deserialize<StackConfig>(fileContent)!;
        }

        public void StartServices(IEnumerable<string>? services)
        {
            var toStart = FilterServices(services);

            if (toStart is not null && toStart.Any())
            {
                foreach (var service in toStart)
                {
                    if (!ServiceExists(service.Start))
                    {
                        Console.WriteLine($"{ANSI_RED}[ERROR] Cannot start {service.Name} - File not found: {service.Start}{ANSI_RESET}");
                        continue;
                    }

                    Console.WriteLine($"{ANSI_GREEN}[START] {service.Name} ({service.Version}){ANSI_RESET}");
                    StartProcess(service.Start);
                }
            }
            else
            {
                Console.WriteLine($"{ANSI_YELLOW}[WARNING] not found the services: {(services is not null ? string.Join(", ", services) : "")}.{ANSI_RESET}");
            }
        }

        public void StopServices(IEnumerable<string>? services)
        {
            var toStop = FilterServices(services);

            if (toStop is not null && toStop.Any())
            {
                foreach (var service in toStop)
                {
                    var exeName = Path.GetFileNameWithoutExtension(service.Start);
                    if (!IsProcessRunning(exeName))
                    {
                        Console.WriteLine($"{ANSI_YELLOW}[WARNING] {service.Name} is not running.{ANSI_RESET}");
                        continue;
                    }

                    Console.WriteLine($"{ANSI_GREEN}[STOP] {service.Name}{ANSI_RESET}");
                    RunCommand(service.Stop);
                }
            }
            else
            {
                Console.WriteLine($"{ANSI_YELLOW}[WARNING] not found the services: {(services is not null ? string.Join(", ", services) : "")}.{ANSI_RESET}");
            }
        }

        public void RestartServices(IEnumerable<string>? services)
        {
            StopServices(services);
            Thread.Sleep(1500);
            StartServices(services);
        }

        public void ListServices()
        {
            foreach (var s in _config.Services)
                Console.WriteLine($"{s.Name} {s.Version}");
        }

        private IEnumerable<ServiceProcess> FilterServices(IEnumerable<string>? names)
        {
            if (names is null || !names.Any()) return _config.Services;
            return _config.Services.Where(s => names.Contains(s.Name, StringComparer.OrdinalIgnoreCase));
        }

        private bool ServiceExists(string path)
        {
            return File.Exists(path);
        }

        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        private void StartProcess(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        private void RunCommand(string cmd)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {cmd}",
                CreateNoWindow = true
            });
        }
    }
}
