using Messenger.Server.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AddUsernameController : ControllerBase
{
    private readonly ChatHub _chatHub;

    public AddUsernameController(ChatHub chatHub)
    {
        _chatHub = chatHub;
    }
    [HttpPost(Name = "AddUsername")]
    public void AddUsername([FromBody] UsernameToConnectionId usernameToChatId)
    {
        _chatHub.AddUsername(usernameToChatId.Username, usernameToChatId.ConnectionId);
    }
}
