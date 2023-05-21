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
        var response = await client.GetAsync(requestUriString);
        return JsonSerializer.Deserialize<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
    }

    private async Task SendUsernameToHubAsync()
    {
        using var client = new HttpClient();
        var requestUriString = $"https://localhost:7240/api/Username/AddUsername";
        await client.PostAsync(requestUriString, new StringContent(JsonSerializer.Serialize(new UsernameToConnectionId()
        {
            ConnectionId = ConnectionId,
            Username = Username
        }), Encoding.UTF8, "application/json"));
    }

    internal async Task<IEnumerable<MessageModel>> RecieveMessages()
    {
        using var client = new HttpClient();
        var requestUriString = $"https://localhost:7240/api/Messages/GetMessages?username={Username}";
        var response = await client.GetAsync(requestUriString);
        var json = await response.Content.ReadAsStringAsync();
        if (json is null)
        {
            return new List<MessageModel>();
        }
        var getMessagesResponse = JsonSerializer.Deserialize<GetMessagesResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (getMessagesResponse is null || getMessagesResponse.Messages is null)
        {
            return new List<MessageModel>();
        }
        return getMessagesResponse.Messages;
    }

    internal async Task SendMessage(MessageModel message)
    {
        await _hubConnection.InvokeAsync($"SendMessage", message.ReceiverUsername, message);
    }
}
