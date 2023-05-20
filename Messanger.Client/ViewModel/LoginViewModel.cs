﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Messanger.Client.VIew;

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
                return _loginCommand ??= new RelayCommand(LoginAsync, x=>!string.IsNullOrWhiteSpace(Username));
            }
        }

        private async void LoginAsync(object obj)
        {
            var mainWindow = new MainWindow() { DataContext = await MainWindowViewModel.BuildViewModelAsync(Username), Title=Username };
            (obj as Window).Close();
            mainWindow.Show();
        }
    }
}
