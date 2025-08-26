using System.Diagnostics;

namespace Lampman.Core.Services
{
    public class StackProcess
    {
        private Process? _process = null;

        public void Start(string exePath, string args = "")
        {
            if (_process != null && !_process.HasExited)
                throw new InvalidOperationException("Process already running.");

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(exePath)
            };

            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            _process.OutputDataReceived += (s, e) => Console.WriteLine("[OUT] " + e.Data);
            _process.ErrorDataReceived += (s, e) => Console.WriteLine("[ERR] " + e.Data);
            _process.Exited += (s, e) => Console.WriteLine($"Process exited with code {_process.ExitCode}");

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public void Stop(string exePath)
        {
            if (_process == null || _process.HasExited)
                return;

            // Send graceful stop (depends on how Apache is configured)
            var stopInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "-k stop",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(stopInfo)?.WaitForExit();

            // If it didn't stop, fallback to Kill
            if (!_process.HasExited)
                _process.Kill();
        }

        public bool IsRunning => _process != null && !_process.HasExited;

        public double GetMemoryUsageMB()
        {
            if (IsRunning && null != _process)
                return _process.WorkingSet64 / 1024.0 / 1024.0;

            return 0;
        }

        public TimeSpan GetCpuTime()
        {
            if (IsRunning && null != _process)
                return _process.TotalProcessorTime;

            return TimeSpan.Zero;
        }
    }
}
