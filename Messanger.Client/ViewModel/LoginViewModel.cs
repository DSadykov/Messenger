using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Messanger.Client.ViewModel
{
    public class LoginViewModel
    {
        private RelayCommand _loginCommand;

        public required string Username { get; set; }
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??= new RelayCommand(Login, x=>!string.IsNullOrWhiteSpace(Username));
            }
        }

        private void Login(object obj)
        {
            var mainWindow = new MainWindow() { DataContext = new MainWindowViewModel(Username), Title=Username };
            (obj as Window).Close();
            mainWindow.Show();
        }
    }
}
