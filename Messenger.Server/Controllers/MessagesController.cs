using Messenger.Core.Models;
using Messenger.Server.Repository;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly MessagesRepository _messagesRepository;

    public MessagesController(MessagesRepository messagesRepository)
    {
        _messagesRepository = messagesRepository;
    }
    [HttpGet("GetMessages")]
    public GetMessagesResponse GetMessages(string username)
    {
        return new() { Messages = _messagesRepository.GetMessages(username) };
    }
    [HttpPost("UploadImage")]
    public async Task UploadImageAsync(ImageModel imageModel)
    {
        await _messagesRepository.UploadImageAsync(imageModel);
    }
    [HttpGet("GetImage")]
    public ImageModel GetImage(Guid imageId)
    {
        ImageModel imageModel = _messagesRepository.GetImage(imageId);
        return imageModel;
    }
}
