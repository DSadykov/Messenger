using Messanger.Client.ViewModel;
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
using Messanger.Client.VIew;
using System.Data;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;

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
            await viewModel.LoadChats(username);
            return viewModel;
        }

        public Visibility ChatWindowVisibility => SelectedChat is null ? Visibility.Collapsed : Visibility.Visible;
        public async Task BuildMessageServiceAsync(string username)
        {
            var url = File.ReadAllText("url.txt");
            _messageService = new MessageService(username, url);
            await _messageService.BeginListeningAsync();
            _messageService.MessageRecieved += RecieveMessageAsync;
        }
        public MainWindowViewModel()
        {
        }

        private async Task LoadChats(string username)
        {
            IEnumerable<MessageModel> tmp = await _messageService.RecieveMessages();
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public ICommand SendMessage => _sendMessage ??= new RelayCommand(PerformSendMessage);

        private async void RecieveMessageAsync(MessageModel message)
        {
            var chat = ChatsList.FirstOrDefault(x => x.ChatName == message.Username);
            ImageModel image = null;
            if (message.ImageId is not null)
            {
                image = await _messageService.RecieveImageAsync(message.ImageId);
            }
            if (chat is null)
            {
                _dispatcher.Invoke(new Action(() =>
                {
                    ChatsList.Add(new ChatModel()
                    {
                        ChatName = message.Username,
                        Messages = new()
                        {
                            new MessageViewModel()
                            {
                                Message = message,
                                HorizontalAlignment = message.Username == Username ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                                Background = message.Username == Username ? Brushes.Green : Brushes.Yellow,
                                ImageVisibility=image is null?Visibility.Collapsed : Visibility.Visible,
                                Image=image is null?null:BitmapToBitmapSource(new Bitmap(new MemoryStream(Convert.FromBase64String(image.ImageBase64))))
                            }
                        }
                    });
                }));
                return;
            }
            _dispatcher.Invoke(new Action(() =>
            {
                chat.Messages.Add(new MessageViewModel()
                {
                    Message = message,
                    HorizontalAlignment = message.Username == Username ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                    Background = message.Username == Username ? Brushes.Green : Brushes.Yellow,
                    ImageVisibility = image is null ? Visibility.Collapsed : Visibility.Visible,
                    Image = image is null ? null : BitmapToBitmapSource(new Bitmap(new MemoryStream(Convert.FromBase64String(image.ImageBase64))))
                });
            }));
        }

        private async void PerformSendMessage(object commandParameter)
        {
            var message = (commandParameter as TextBox)?.Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            var messageModel = new MessageModel()
            {
                DateSent = DateTime.Now,
                Username = Username,
                ReceiverUsername = SelectedChat.ChatName,
                Message = message,
                Id = Guid.NewGuid()
            };
            ImageModel imageModel = null;
            if (SelectedImage is not null)
            {
                imageModel = new() { ImageBase64 = Convert.ToBase64String( BitmapSourceToByteArray(SelectedImage)), Id = Guid.NewGuid(), MessageId = messageModel.Id };
                messageModel.ImageId = imageModel.Id;
                await _messageService.SendMessage(messageModel, imageModel);
            }
            else
            {
                await _messageService.SendMessage(messageModel);
            }
            SelectedChat.Messages.Add(new MessageViewModel()
            {
                Message = messageModel,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = Brushes.Green,
                ImageVisibility = imageModel is null ? Visibility.Collapsed : Visibility.Visible,
                Image = imageModel is null ? null : SelectedImage.Clone()
            });
            (commandParameter as TextBox).Text = "";
            SelectedImage = null;
            NotifyPropertyChanged(nameof(SelectedImage));
        }

        private RelayCommand _addNewChatCommand;
        public ICommand AddNewChatCommand => _addNewChatCommand ??= new RelayCommand(AddNewChat);

        private async void AddNewChat(object commandParameter)
        {
            var onlineUsers = (await _messageService.GetOnlineUsers()).Except(new List<string>() { Username });

            new AddNewChatView() { DataContext = new AddNewChatViewModel(this, onlineUsers) }.Show();
        }

        internal void AddNewChat(string v)
        {
            _dispatcher.Invoke(new Action(() =>
            {
                ChatsList.Add(new ChatModel()
                {
                    ChatName = v,
                    Messages = new()
                });
            }));
        }

        private RelayCommand _removeSelectedImage;
        public BitmapSource SelectedImage { get; set; }
        public ICommand RemoveSelectedImage => _removeSelectedImage ??= new RelayCommand(PerformRemoveSelectedImage);

        private void PerformRemoveSelectedImage(object commandParameter)
        {
            SelectedImage = null;
            AttachedImageVisibility = Visibility.Collapsed;
            NotifyPropertyChanged(nameof(AttachedImageVisibility));
            NotifyPropertyChanged(nameof(SelectedImage));
        }

        private RelayCommand _addImageToMessage;
        public ICommand AddImageToMessage => _addImageToMessage ??= new RelayCommand(PerformAddImageToMessage);

        private void PerformAddImageToMessage(object commandParameter)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "Image Files(*.PNG;*.JPG;)|*.PNG;*.JPG",
                Title = "Выберите картинку"
            };
            if (!(bool)dialog.ShowDialog())
            {
                return;
            }
            var imageBytes = File.ReadAllBytes(dialog.FileName);
            SelectedImage = BitmapToBitmapSource(new Bitmap(new MemoryStream(imageBytes)));
            AttachedImageVisibility = Visibility.Visible;
            NotifyPropertyChanged(nameof(AttachedImageVisibility));
            NotifyPropertyChanged(nameof(SelectedImage));
        }
        public Visibility AttachedImageVisibility { get; set; } = Visibility.Collapsed;
        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }
        public static byte[] BitmapSourceToByteArray(BitmapSource bitmap)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.QualityLevel = 100;
            // byte[] bit = new byte[0];
            using (MemoryStream stream = new())
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                byte[] bit = stream.ToArray();
                stream.Close();
                return bit;
            }
        }
    }
}
