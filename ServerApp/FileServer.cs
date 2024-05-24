using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    public class FileServer
    {
        private readonly int port;
        private string filesDirectoryPath = string.Empty;

        public FileServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            try
            {
                var drives = DriveInfo.GetDrives().Select((drive) => drive.Name).ToList();
                // Try to create storage directory on first non-system disk...
                if (drives.Count >= 2)
                {
                    filesDirectoryPath = Path.Combine(drives[1], "ClientFiles");
                }
                // ...or on single disk
                else
                {
                    filesDirectoryPath = Path.Combine(drives[0], "ClientFiles");
                }
                Directory.CreateDirectory(filesDirectoryPath);
            }
            catch
            {
                // If some error, create storage dir in app root folder
                filesDirectoryPath = "ClientFiles";
                Directory.CreateDirectory(filesDirectoryPath);
            }

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            LogMessage("Server started...");

            while (true)
            {
                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                using (var reader = new BinaryReader(stream))
                {
                    var isDirectory = reader.ReadBoolean();
                    var fileName = reader.ReadString();
                    var internalPath = reader.ReadString();

                    LogMessage($"Receiving file {fileName}");
                    var path = Path.Combine(filesDirectoryPath, internalPath, fileName);

                    // Receive file
                    if (isDirectory == false)
                    {
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            reader.BaseStream.CopyTo(fileStream);
                        }
                    }
                    // Receive directory
                    else
                    {
                        Directory.CreateDirectory(path);
                    }

                    LogMessage($"File {fileName} received");
                }
            }
        }

        private void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}

