using System.Collections.Generic;

using Messenger.Core.Models;

using Microsoft.AspNetCore.SignalR;

namespace Messenger.Server.Services;

public class ChatHub : Hub
{
    private Dictionary<string,string> _usernameToConnectionId= new Dictionary<string,string>();
    public void AddUsername(string username, string chatId)
    {
        if (_usernameToConnectionId.ContainsKey(username))
        {
            _usernameToConnectionId[username] = chatId;
            return;
        }
        _usernameToConnectionId.Add(username, chatId);
    }
    public IEnumerable<string> GetUsernameList()
    {
        return _usernameToConnectionId.Keys;
    }
    public async Task SendMessage(string userName, MessageModel message)
    {
        await Clients.Client(_usernameToConnectionId[userName]).SendAsync("SendMessage", message);
    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _usernameToConnectionId.Remove(_usernameToConnectionId.FirstOrDefault(x => x.Value == Context.ConnectionId).Key);
        return base.OnDisconnectedAsync(exception);
    }
}
