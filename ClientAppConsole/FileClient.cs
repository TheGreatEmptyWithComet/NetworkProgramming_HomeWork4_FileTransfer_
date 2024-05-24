using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class FileClient
    {
        private readonly string host;
        private readonly int port;
        private string directoryPath;
        private bool innerDirectoryIsShown = false;
        private List<string> fileEntities = new List<string>();

        public FileClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Send()
        {
            while (true)
            {
                Console.Clear();

                if (innerDirectoryIsShown == false)
                {
                    ShowMessage($"Input directory path");
                    directoryPath = Console.ReadLine();

                    if (!Directory.Exists(directoryPath))
                    {
                        ShowMessage($"Wrong path");
                        Console.ReadKey();
                        continue;
                    }
                }

                Console.WriteLine($"\nFiles in directory:");
                ReadAndShowFiles();
                Console.WriteLine($"\nInput file number to copy or 0 to exit");

                if (!int.TryParse(Console.ReadLine(), out int fileNumber) || fileNumber == 0 || fileNumber > fileEntities.Count || fileNumber < 1)
                {
                    continue;
                }

                string filePath = fileEntities[fileNumber - 1];

                if (PathIsDirectory(filePath))
                {
                    directoryPath = filePath;
                    innerDirectoryIsShown = true;
                    continue;
                }

                using (TcpClient tcpClient = new TcpClient(host, port))
                using (var stream = tcpClient.GetStream())
                using (var writer = new BinaryWriter(stream))
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    writer.Write(Path.GetFileName(filePath));
                    writer.Write(fileStream.Length);

                    ShowMessage($"Sending file {Path.GetFileName(filePath)} / size: {fileStream.Length}");

                    fileStream.CopyTo(stream);

                    ShowMessage($"File {Path.GetFileName(filePath)} sent");
                }

                ShowMessage($"Press any key to copy next file");
                innerDirectoryIsShown = false;
                Console.ReadKey();
            }
        }

        private void ReadAndShowFiles()
        {
            fileEntities = Directory.GetFileSystemEntries(directoryPath).ToList();

            List<string> directories = fileEntities.Where((f) => File.GetAttributes(f).HasFlag(FileAttributes.Directory)).OrderBy((f) => f).ToList();
            List<string> files = fileEntities.Except(directories).OrderBy((f) => f).ToList();
            
            fileEntities = new List<string>();
            fileEntities.AddRange(directories);
            fileEntities.AddRange(files);

            int count = 1;
            foreach (var file in fileEntities)
            {
                Console.WriteLine($"#{count++} {Path.GetFileName(file)}");
            }
        }

        private bool PathIsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        private void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
