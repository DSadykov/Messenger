using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

using Messanger.Client.ViewModel;

namespace Messanger.Client.Model;

public class ChatModel
{
    public string ChatName { get; set; }
    public ObservableCollection<MessageViewModel> Messages { get; set; }
}