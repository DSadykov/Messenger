using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Accessibility;

using Messanger.Client.ViewModel;

using Messenger.Core.Models;
using Messenger.Server.Controllers;

using Microsoft.AspNetCore.SignalR.Client;

namespace Messanger.Client.Services;

public class MessageService
{
    public HubConnection _hubConnection;
    public string ConnectionId => _hubConnection.ConnectionId;
    public MessageService(string username)
    {
        Username = username;
    }

    public Action<MessageModel> MessageRecieved { get; set; }
    public string Username { get; }

    internal async Task BeginListeningAsync()
    {
        _hubConnection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:7240/chatHub")
                        .Build();
                
        _hubConnection.On("SendMessage", (MessageModel x) =>
        {
            MessageRecieved(x);
        });
        await _hubConnection.StartAsync();
        await SendUsernameToHubAsync();
    }
    internal async Task<IEnumerable<string>> GetOnlineUsers()
    {

        using var client = new HttpClient();
        var requestUriString = $"https://localhost:7240/api/Username/GetUsernames";
        var response= await client.GetAsync(requestUriString);
        return JsonSerializer.Deserialize<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
    }

    private async Task SendUsernameToHubAsync()
    {
        using var client = new HttpClient();
        var requestUriString = $"https://localhost:7240/api/Username/AddUsername";
        await client.PostAsync(requestUriString, new StringContent(JsonSerializer.Serialize(new UsernameToConnectionId()
        {
            ConnectionId=ConnectionId, Username=Username
        }), Encoding.UTF8, "application/json"));
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
        _hubConnection.InvokeAsync($"SendMessage", messageTo, new MessageModel()
        {
            DateSent = DateTime.Now,
            Username = Username,
            ReceiverUsername = messageTo,
            Message = message,
            Id = Guid.NewGuid()
        });
    }
}
