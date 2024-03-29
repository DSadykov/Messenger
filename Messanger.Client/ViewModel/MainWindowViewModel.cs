﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Messanger.Client.Helpers;
using Messanger.Client.Model;
using Messanger.Client.Services;
using Messanger.Client.VIew;

using Messenger.Core.Models;

using Microsoft.Win32;

using Brushes = System.Windows.Media.Brushes;

namespace Messanger.Client.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public string Username { get; private set; }
    public ObservableCollection<ChatModel> ChatsList { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;

    private Dispatcher _dispatcher;
    private readonly ImageHelper _imageHelper = new();
    private MessageService _messageService;
    private ChatModel _selectedChat;
    private BitmapSource? _selectedImage;


    public Visibility ChatWindowVisibility => SelectedChat is null ? Visibility.Collapsed : Visibility.Visible;

    public Visibility AttachedImageVisibility => SelectedImage is null ? Visibility.Collapsed : Visibility.Visible;



    public ICommand SendMessage => _sendMessage ??= new RelayCommand(PerformSendMessage);
    public ICommand AddNewChatCommand => _addNewChatCommand ??= new RelayCommand(AddNewChat);
    public ICommand RemoveSelectedImage => _removeSelectedImage ??= new RelayCommand(PerformRemoveSelectedImage);
    public ICommand AddImageToMessage => _addImageToMessage ??= new RelayCommand(PerformAddImageToMessage);


    private ICommand _sendMessage;
    private ICommand _addNewChatCommand;
    private ICommand _removeSelectedImage;
    private ICommand _addImageToMessage;


    public BitmapSource? SelectedImage
    {
        get => _selectedImage; set
        {
            _selectedImage = value;
            NotifyPropertyChanged(nameof(SelectedImage));
            NotifyPropertyChanged(nameof(AttachedImageVisibility));

        }
    }
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


    public static async Task<MainWindowViewModel> BuildViewModelAsync(string username)
    {
        MainWindowViewModel viewModel = new()
        {
            Username = username
        };
        await viewModel.BuildMessageServiceAsync(username);
        await viewModel.LoadChats(username);
        return viewModel;
    }

    public async Task BuildMessageServiceAsync(string username)
    {
        var url = File.ReadAllText("url.txt");
        _messageService = new MessageService(username, url);
        await _messageService.BeginListeningAsync();
        _messageService.MessageRecieved += RecieveMessageAsync;
    }

    private async Task LoadChats(string username)
    {
        IEnumerable<MessageModel> tmp = await _messageService.RecieveMessages();
        Dictionary<Guid, BitmapSource> images = new();
        foreach (Guid imageId in tmp.Where(x => x.ImageId is not null).Select(x => (Guid)x.ImageId))
        {
            images[imageId] = _imageHelper.Base64StringToBitmapSource((await _messageService.RecieveImageAsync(imageId)).ImageBase64);
        }
        IEnumerable<ChatModel> chats = tmp.GroupBy(x =>
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
                Image = y.ImageId is not null ? images[(Guid)y.ImageId] : null,
                ImageVisibility = y.ImageId is null ? Visibility.Collapsed : Visibility.Visible,
            }))
        });
        _dispatcher = Dispatcher.CurrentDispatcher;
        ChatsList = new(chats);
    }


    protected void NotifyPropertyChanged(string info)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    private async void RecieveMessageAsync(MessageModel message)
    {
        ChatModel? chat = ChatsList.FirstOrDefault(x => x.ChatName == message.Username);
        ImageModel? image = null;
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
                            Image=image is null?null:_imageHelper.Base64StringToBitmapSource(image.ImageBase64)
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
                Image = image is null ? null : _imageHelper.Base64StringToBitmapSource(image.ImageBase64)
            });
        }));
    }

    private async void PerformSendMessage(object commandParameter)
    {
        var message = (commandParameter as TextBox)?.Text;
        if (string.IsNullOrEmpty(message) && SelectedImage is null)
        {
            return;
        }
        MessageModel messageModel = new()
        {
            DateSent = DateTime.Now,
            Username = Username,
            ReceiverUsername = SelectedChat.ChatName,
            Message = message,
            Id = Guid.NewGuid()
        };
        ImageModel? imageModel = null;
        if (SelectedImage is not null)
        {
            imageModel = new() { ImageBase64 = _imageHelper.BitmapSourceToBase64String(SelectedImage), Id = Guid.NewGuid(), MessageId = messageModel.Id };
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
    }

    private async void AddNewChat(object commandParameter)
    {
        IEnumerable<string> onlineUsers = (await _messageService.GetOnlineUsers()).Except(new List<string>() { Username });

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

    private void PerformRemoveSelectedImage(object commandParameter)
    {
        SelectedImage = null;
    }


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
        SelectedImage = _imageHelper.BinaryToBitmapSource(imageBytes);
    }
}
