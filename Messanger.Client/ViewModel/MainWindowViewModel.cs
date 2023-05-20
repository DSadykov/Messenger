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
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;

namespace Messanger.Client.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public string Username { get; private set; }
        private Dispatcher _dispatcher;
        public static async Task<MainWindowViewModel> BuildViewModelAsync(string username)
        {
            var viewModel = new MainWindowViewModel
            {
                Username = username
            };
            await viewModel.BuildMessageServiceAsync(username);
            viewModel.LoadChats(username);
            return viewModel;
        }

        public Visibility ChatWindowVisibility => SelectedChat is null ? Visibility.Collapsed : Visibility.Visible;
        public async Task BuildMessageServiceAsync(string username)
        {
            _messageService = new MessageService(username);
            await _messageService.BeginListeningAsync();
            _messageService.MessageRecieved += RecieveMessageAsync;
        }
        public MainWindowViewModel()
        {
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
            _dispatcher = Dispatcher.CurrentDispatcher;
            ChatsList = new(chats);
        }



        public ObservableCollection<ChatModel> ChatsList { get; set; }
        public ChatModel SelectedChat
        {
            get => _selectedChat;
            set
            {
                _selectedChat = value;

                NotifyPropertyChanged(nameof(SelectedChat));
                NotifyPropertyChanged(nameof(ChatWindowVisibility));
            }
        }

        private RelayCommand _sendMessage;
        private MessageService _messageService;
        private ChatModel _selectedChat;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ICommand SendMessage => _sendMessage ??= new RelayCommand(PerformSendMessage);

        private void RecieveMessageAsync(MessageModel message)
        {
            var chat = ChatsList.FirstOrDefault(x => x.ChatName == message.Username);
            if (chat is null)
            {
                return;
            }
            _dispatcher.Invoke(new Action(() =>
            {
                chat.Messages.Add(new MessageViewModel()
                {
                    Message = message,
                    HorizontalAlignment = message.Username == Username ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                    Background = message.Username == Username ? Brushes.Green : Brushes.Yellow,
                });
            }));
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
