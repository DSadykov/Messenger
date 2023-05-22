using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Messenger.Core.Models;
using Messenger.Server.Controllers;

using Microsoft.AspNetCore.SignalR.Client;

namespace Messanger.Client.Services;

public class MessageService
{
    public HubConnection _hubConnection;
    private readonly string _url;

    public string ConnectionId => _hubConnection.ConnectionId;
    public MessageService(string username, string url)
    {
        Username = username;
        _url = url;
    }

    public Action<MessageModel> MessageRecieved { get; set; }
    public string Username { get; }

    internal async Task BeginListeningAsync()
    {
        _hubConnection = new HubConnectionBuilder()
                        .WithUrl($"{_url}/chatHub")
                        .Build();

        _ = _hubConnection.On("SendMessage", (MessageModel x) =>
        {
            MessageRecieved(x);
        });
        await _hubConnection.StartAsync();
        await SendUsernameToHubAsync();
    }
    internal async Task<IEnumerable<string>> GetOnlineUsers()
    {

        using HttpClient client = new();
        var requestUriString = $"{_url}/api/Username/GetUsernames";
        HttpResponseMessage response = await client.GetAsync(requestUriString);
        return JsonSerializer.Deserialize<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
    }

    private async Task SendUsernameToHubAsync()
    {
        using HttpClient client = new();
        var requestUriString = $"{_url}/api/Username/AddUsername";
        _ = await client.PostAsync(requestUriString, new StringContent(JsonSerializer.Serialize(new UsernameToConnectionId()
        {
            ConnectionId = ConnectionId,
            Username = Username
        }), Encoding.UTF8, "application/json"));
    }

    internal async Task<IEnumerable<MessageModel>> RecieveMessages()
    {
        using HttpClient client = new();
        var requestUriString = $"{_url}/api/Messages/GetMessages?username={Username}";
        HttpResponseMessage response = await client.GetAsync(requestUriString);
        var json = await response.Content.ReadAsStringAsync();
        if (json is null)
        {
            return new List<MessageModel>();
        }
        GetMessagesResponse? getMessagesResponse = JsonSerializer.Deserialize<GetMessagesResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return getMessagesResponse is null || getMessagesResponse.Messages is null ? new List<MessageModel>() : getMessagesResponse.Messages;
    }

    internal async Task SendMessage(MessageModel message)
    {
        await _hubConnection.InvokeAsync($"SendMessage", message.ReceiverUsername, message);
    }

    internal async Task SendMessage(MessageModel messageModel, ImageModel imageModel)
    {
        using HttpClient client = new();
        var requestUriString = $"{_url}/api/Messages/UploadImage";
        HttpResponseMessage response = await client.PostAsJsonAsync(requestUriString, imageModel);
        await SendMessage(messageModel);
    }

    internal async Task<ImageModel> RecieveImageAsync(Guid? imageId)
    {
        using HttpClient client = new();
        var requestUriString = $"{_url}/api/Messages/GetImage?imageId={imageId}";
        HttpResponseMessage response = await client.GetAsync(requestUriString);
        return await response.Content.ReadFromJsonAsync<ImageModel>();
    }
}
