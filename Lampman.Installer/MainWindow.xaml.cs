using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Threading.Tasks;

namespace Lampman.Installer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtInstallPath.Text = dialog.SelectedPath;
            }
        }

        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            string installPath = txtInstallPath.Text.Trim();
            if (string.IsNullOrEmpty(installPath))
            {
                System.Windows.Forms.MessageBox.Show("Please select an installation path.");
                return;
            }

            btnInstall.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                // Create directory
                Directory.CreateDirectory(installPath);

                // Copy release build files
                string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Release");
                foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string dest = Path.Combine(installPath, file.Substring(sourcePath.Length + 1));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(file, dest, true);
                }

                // Add to PATH if selected
                if (chkAddToPath.IsChecked == true)
                {
                    AddToSystemPath(installPath);
                }

                // Create default stack.json if missing
                string stackFile = Path.Combine(installPath, "stack.json");
                if (!File.Exists(stackFile))
                {
                    File.WriteAllText(stackFile, @"{
                    ""Services"": []
                    }");
                }
            });

            System.Windows.Forms.MessageBox.Show("Installation completed successfully!", "Lampman Installer");

            if (chkRunAfter.IsChecked == true)
            {
                Process.Start(Path.Combine(txtInstallPath.Text, "lampman.exe"));
            }

            Close();
        }

        private void AddToSystemPath(string folderPath)
        {
            const string name = "Path";
            string currentPath = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine) ?? "";
            if (!currentPath.Contains(folderPath, StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable(name, currentPath + ";" + folderPath, EnvironmentVariableTarget.Machine);
            }
        }
    }
}
