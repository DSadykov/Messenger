using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messanger.Client.ViewModel
{
    class MainWindowViewModel
    {
        public MainWindowViewModel(string username)
        {
            Username = username;
        }

        public string Username { get; }
    }
}
