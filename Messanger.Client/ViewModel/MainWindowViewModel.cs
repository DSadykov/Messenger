using Messanger.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Messanger.Client.Model;
using System.Windows.Input;
using Messanger.Client.Services;
using Messenger.Core.Models;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Controls;

namespace Messanger.Client.ViewModel
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel(string username)
        {
            _messageService = new MessageService() { Username = username };
            LoadChats(username);
            _messageService.BeginListening();
        }

        private void LoadChats(string username)
        {
            IEnumerable<MessageModel> tmp = _messageService.RecieveMessages();
            var chats = tmp.GroupBy(x => 
            {
                if (x.ReceiverUsername == username)
                {
                    return x.Username;
                };
                return x.ReceiverUsername;
            }).Select(x => new ChatModel()
            {
                ChatName = x.Key,
                Messages = new(x.OrderBy(x => x.DateSent).Select(y => new MessageViewModel()
                {
                    Message = y,
                    HorizontalAlignment = y.Username == username ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                    Background = y.Username == username ? Brushes.Green : Brushes.Yellow,
                }))});
            ChatsList = new(chats);
        }

        public MainWindowViewModel()
        {
            _messageService = new MessageService() { Username = "Me" };
            LoadChats("Me");
        }


        public ObservableCollection<ChatModel> ChatsList { get; set; }
        public ChatModel SelectedChat { get; set; }

        private RelayCommand _sendMessage;
        private readonly MessageService _messageService;

        public ICommand SendMessage => _sendMessage ??= new RelayCommand(PerformSendMessage);

        private void PerformSendMessage(object commandParameter)
        {
            var message = (commandParameter as TextBox).Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            _messageService.SendMessage(message, SelectedChat.ChatName);
            (commandParameter as TextBox).Text = "";
        }
    }
}
