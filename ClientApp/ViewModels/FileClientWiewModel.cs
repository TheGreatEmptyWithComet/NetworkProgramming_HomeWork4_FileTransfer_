using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientApp
{
    public class FileClientWiewModel
    {
        public FileClient FileClient { get; set; }

        public ICommand OpenDirectoryCommand { get; set; }
        public ICommand CopyFileCommand { get; set; }


        public FileClientWiewModel()
        {
            FileClient = new FileClient("localhost", 8800);

            InitCommands();
        }


        private void InitCommands()
        {
            OpenDirectoryCommand = new RelayCommand<object>((p) => OpenDirectory(p));
            CopyFileCommand = new RelayCommand<object>((p) => CopyFile(p));
        }

        private void OpenDirectory(object param)
        {
            if (param is string name)
            {
                FileClient.OpenDirectory(name);
            }
        }
        private void CopyFile(object param)
        {
            if (param is string name && !string.IsNullOrEmpty(name))
            {
                FileClient.CopyFile(name);
            }
        }
    }
}
