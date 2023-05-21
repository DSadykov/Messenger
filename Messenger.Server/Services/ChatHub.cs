using System.Collections.Generic;

using Messenger.Core.Models;
using Messenger.Server.Repository;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Messenger.Server.Services;

public class ChatHub : Hub
{
    public ChatHub(DbContextOptions<MessagesDbContext> dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }
    private Dictionary<string, string> _usernameToConnectionId = new();
    private readonly DbContextOptions<MessagesDbContext> _dbContextOptions;

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
        using var context = new MessagesDbContext(_dbContextOptions);
        var messagesRepository = new MessagesRepository(context);
        if (!_usernameToConnectionId.ContainsKey(userName))
        {
            await messagesRepository.AddMessage(message);
            return;
        }
        await Clients.Client(_usernameToConnectionId[userName]).SendAsync("SendMessage", message);
        await messagesRepository.AddMessage(message);
    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _usernameToConnectionId.Remove(_usernameToConnectionId.FirstOrDefault(x => x.Value == Context.ConnectionId).Key);
        return base.OnDisconnectedAsync(exception);
    }
}
