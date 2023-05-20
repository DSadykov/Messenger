using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

using Messanger.Client.Model;
using System.Windows.Input;
using Messanger.Client.Services;
using Messenger.Core.Models;
using System.Windows.Media;
using System.Windows.Controls;
using System.Net.Http.Headers;

namespace Messanger.Client.ViewModel
{
    public class MainWindowViewModel
    {
        public string Username { get; private set; }
        public MainWindowViewModel(string username)
        {
            Username = username;
            _messageService = new MessageService(username);
            LoadChats(username);
            _messageService.BeginListening();
            _messageService.MessageRecieved += RecieveMessage;
        }

        public MainWindowViewModel()
        {
            _messageService = new MessageService("Me");
            LoadChats("Me");
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
                }))
            });
            ChatsList = new(chats);
        }



        public ObservableCollection<ChatModel> ChatsList { get; set; }
        public ChatModel SelectedChat { get; set; }

        private RelayCommand _sendMessage;
        private readonly MessageService _messageService;

        public ICommand SendMessage => _sendMessage ??= new RelayCommand(PerformSendMessage);

        private void RecieveMessage(MessageModel message)
        {
            var chat = ChatsList.FirstOrDefault(x => x.ChatName == message.Username);
            if(chat is null)
            {
                return;
            }
            chat.Messages.Add(new MessageViewModel()
            {
                Message = message,
                HorizontalAlignment = message.Username == Username ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                Background = message.Username == Username ? Brushes.Green : Brushes.Yellow,
            });
        }

        private void PerformSendMessage(object commandParameter)
        {
            var message = (commandParameter as TextBox)?.Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            _messageService.SendMessage(message, SelectedChat.ChatName);
            (commandParameter as TextBox).Text = "";
        }
    }
}
