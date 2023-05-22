using System.Collections.ObjectModel;

using Messanger.Client.ViewModel;

namespace Messanger.Client.Model;

public class ChatModel
{
    public required string ChatName { get; set; }
    public required ObservableCollection<MessageViewModel> Messages { get; set; }
}