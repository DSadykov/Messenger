using Messenger.Server.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsernameController : ControllerBase
{
    private readonly ChatHub _chatHub;

    public UsernameController(ChatHub chatHub)
    {
        _chatHub = chatHub;
    }
    [HttpPost("AddUsername")]
    public void AddUsername([FromBody] UsernameToConnectionId usernameToChatId)
    {
        _chatHub.AddUsername(usernameToChatId.Username, usernameToChatId.ConnectionId);
    }
    [HttpGet("GetUsernames")]
    public IEnumerable<string> GetUsernames()
    {
        return _chatHub.GetUsernameList();
    }
}
