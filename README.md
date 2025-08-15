# Lampman CLI

**Lampman** is a lightweight, configurable command-line tool for managing local development stacks on Windows.  
It allows you to start, stop, and restart multiple services (Apache, MySQL, PHP, etc.) with ease, using a simple `stack.json` configuration file.  

Designed for flexibility — you can define multiple versions of each service and expand the stack with custom tools.  
Future plans include version switching, automatic SSL, Virtual Host manager, local HTML dashboard GUI, and more.

---

## Features
- **Start/Stop/Restart** services from the command line
- **Customizable stack** via `stack.json`
- Checks if binaries exist before starting
- Warns if service is not running when stopping
- Portable: Works with installed or portable binaries
- Ready for expansion to multi-version stacks and GUI control

---

## Installation

1. **Clone the repository**
```powershell
git clone https://github.com/samuel-alvarez-quintero/Lampman.git
cd lampman
```

2. **Build the project**
```powershell
dotnet build
```

3. **(Optional) Create a global command**
- Copy the built executable from:
  ```
  Lampman.Cli\bin\Debug\net8.0\Lampman.Cli.exe
  ```
  into a folder in your `PATH` (e.g., `C:\tools`).

---

## Configuration

On first run, Lampman will automatically create a **default** `stack.json` file if it doesn't exist:

```json
{
  "services": [
    {
      "name": "Apache",
      "version": "2.4",
      "start": "C:\\lampman\\apache-2.4\\bin\\httpd.exe",
      "stop": "taskkill /IM httpd.exe /F"
    },
    {
      "name": "MySQL",
      "version": "8.0",
      "start": "C:\\lampman\\mysql-8.0\\bin\\mysqld.exe",
      "stop": "taskkill /IM mysqld.exe /F"
    },
    {
      "name": "PHP",
      "version": "8.2",
      "start": "C:\\lampman\\php-8.2\\php-cgi.exe -b 127.0.0.1:9000",
      "stop": "taskkill /IM php-cgi.exe /F"
    }
  ]
}
```

Edit the `start` and `stop` paths to match your setup.

---

## Usage

From the CLI, run:

```powershell
# List configured services
lampman list

# Start all services or a specific one
lampman start [apache|mysql]

# Stop all services or a specific one
lampman stop [apache|mysql]

# Restart all services or a specific one
lampman restart [apache|mysql]
```

---

## Roadmap

- [ ] Multi-version PHP/MySQL/Apache switching
- [ ] Portable default stack bundle
- [ ] PID tracking for precise stop/restart
- [ ] Built-in local HTTP API
- [ ] HTML/JS Dashboard
- [ ] Cross-platform support (Linux/macOS via .NET)
- [ ] No dependencies on local services or daemons like Docker, Vagrant, MySQL, Nginx, Apache, and others.

---

## Contributing

1. Fork the repository
2. Create a new branch (`feature/awesome-feature`)
3. Commit changes
4. Push to your branch
5. Open a Pull Request

---

## License
MIT License — see [LICENSE](LICENSE) for details.

---
