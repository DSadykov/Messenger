using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Messanger.Client.ViewModel;

using Messenger.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

namespace Messanger.Client.Services;

public class MessageService
{
    private HubConnection _hubConnection;

    public MessageService(string username)
    {
        Username = username;
    }

    public Action<MessageModel> MessageRecieved { get; internal set; }
    public string Username { get; }

    internal void BeginListening()
    {
        _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53353/ChatHub")
                .Build();
        _hubConnection.StartAsync();
        _hubConnection.On($"MessageTo{Username}", MessageRecieved);
    }

    internal IEnumerable<MessageModel> RecieveMessages()
    {
        return new List<MessageModel>
        {
            new()
            {

                    Message="Pepega2",
                    DateSent=DateTime.Now,
                    Username="Me",
                    ReceiverUsername="You"

            },
            new()
            {

                    Message="Pepega3",
                    DateSent=DateTime.Now.AddMinutes(5),
                    Username="Me",

                    ReceiverUsername="You"

            },new()
            {

                    Message="Pepega4",
                    DateSent=DateTime.Now.AddMinutes(-2),
                    Username="Me",
                    ReceiverUsername="You"

            }
            ,new()
            {

                    Message="Pepega1",
                    DateSent=DateTime.Now.AddMinutes(1),
                    Username="You",
                    ReceiverUsername="Me"

            },new()
            {

                    Message="Pepega2",
                    DateSent=DateTime.Now.AddMinutes(-1),
                    Username="You",
                    ReceiverUsername="Me"

            },
        };


    }

    internal void SendMessage(string message, string messageTo)
    {
        _hubConnection.SendAsync($"MessageTo{messageTo}",message);
    }
}
