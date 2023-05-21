using Messanger.Client.ViewModel;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Messanger.Client.ViewModel;
internal class AddNewChatViewModel
{
    public AddNewChatViewModel(MainWindowViewModel mainWindowViewModel, IEnumerable<string> onlineUsers)
    {
        _mainWindowViewModel = mainWindowViewModel;
        OnlineUsers = new(onlineUsers);
    }
    public ObservableCollection<string> OnlineUsers { get; set; }
    public string SelectedUser { get; set; }
    private RelayCommand _addNewChat;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public ICommand AddNewChat => _addNewChat ??= new RelayCommand(PerformAddNewChat, x=> SelectedUser is not null);

    private void PerformAddNewChat(object commandParameter)
    {
        _mainWindowViewModel.AddNewChat(SelectedUser);
        (commandParameter as Window).Close();
    }
}
