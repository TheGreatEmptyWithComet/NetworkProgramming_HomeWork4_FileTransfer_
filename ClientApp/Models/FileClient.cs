using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class FileClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly string parentDirectoryName = "..";
        private readonly string host;
        private readonly int port;
        private string currentDirectoryPath = string.Empty;
        private string logMessage = string.Empty;
        public string LogMessage
        {
            get => logMessage;
            set
            {
                logMessage = value;
                NotifyPropertyChanged(nameof(LogMessage));
            }
        }

        public ObservableCollection<string> FileEntities { get; set; } = new ObservableCollection<string>();

        public FileClient(string host, int port)
        {
            this.host = host;
            this.port = port;

            ShowDrives();
        }

        private void ShowDrives()
        {
            FileEntities.Clear();
            DriveInfo.GetDrives().Select((drive) => drive.Name).ToList().ForEach((i) => FileEntities.Add(i));

        }
        public void OpenDirectory(string name)
        {
            string fullName = Path.Combine(currentDirectoryPath, name);

            try
            {
                // case new inner directory is opened
                if (name != parentDirectoryName && File.GetAttributes(fullName).HasFlag(FileAttributes.Directory))
                {
                    currentDirectoryPath = fullName;
                    FileEntities.Clear();

                    // Add parend directory mark
                    FileEntities.Add("..");

                    // Add file entities
                    var entities = Directory.GetFileSystemEntries(fullName);
                    List<string> directories = entities.Where((f) => IsDirectory(f)).OrderBy((f) => f).ToList();
                    List<string> files = entities.Except(directories).OrderBy((f) => f).ToList();

                    directories.Select((i) => Path.GetFileName(i)).ToList().ForEach((i) => FileEntities.Add(i));
                    files.Select((i) => Path.GetFileName(i)).ToList().ForEach((i) => FileEntities.Add(i));

                }
                // case move one level up
                else if (name == "..")
                {
                    // case level up entity is a directory
                    if (new DirectoryInfo(currentDirectoryPath).Parent != null)
                    {
                        currentDirectoryPath = new DirectoryInfo(currentDirectoryPath).Parent.FullName;
                        OpenDirectory(string.Empty);
                    }
                    // case level up entity is a drive
                    else
                    {
                        currentDirectoryPath = string.Empty;
                        ShowDrives();
                    }
                }
            }
            catch { };

        }
        public async void CopyFile(string name)
        {
            if (IsDirectory(Path.Combine(currentDirectoryPath, name)))
            {
                await SendFileAsync(name, string.Empty, true);
            }
            else
            {
                await SendFileAsync(name, string.Empty);
            }
        }
        private async Task SendFileAsync(string name, string internalPath, bool isDirectory = false)
        {
            string fullName = Path.Combine(currentDirectoryPath, internalPath, name);

            await Task.Run(() =>
            {
                using (TcpClient tcpClient = new TcpClient(host, port))
                using (var stream = tcpClient.GetStream())
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(isDirectory);
                    writer.Write(name);
                    writer.Write(internalPath);

                    if (isDirectory == false)
                    {
                        using (var fileStream = new FileStream(fullName, FileMode.Open))
                        {
                            fileStream.CopyTo(stream);

                            LogMessage = $"file {name} sent";
                        }
                    }
                    else
                    {
                        LogMessage = $"Directory {name} sent";

                        // If directory - run method again recursievly
                        // Get current directory entities
                        var entities = Directory.GetFileSystemEntries(fullName);
                        List<string> directories = entities.Where((f) => IsDirectory(f)).Select((f) => Path.GetFileName(f)).ToList();
                        List<string> files = entities.Where((f) => !IsDirectory(f)).Select((f) => Path.GetFileName(f)).ToList();

                        internalPath = Path.Combine(internalPath, name);
                        // Copy files
                        files.ForEach(async (f) => await SendFileAsync(f, internalPath));
                        // Copy directories
                        directories.ForEach(async (f) => await SendFileAsync(f, internalPath, true));
                    }
                }
            });
        }

        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private bool IsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
    }
}
