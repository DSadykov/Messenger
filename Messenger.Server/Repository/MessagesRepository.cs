using Messenger.Core.Models;

namespace Messenger.Server.Repository;

public class MessagesRepository
{
    private readonly MessagesDbContext _messagesDbContext;

    public MessagesRepository(MessagesDbContext messagesDbContext)
	{
        _messagesDbContext = messagesDbContext;
    }

    public IEnumerable<MessageModel> GetMessages(string username)
    {
        return _messagesDbContext.Messages.Where(m => m.Username == username|| m.ReceiverUsername == username);
    }

    public async Task AddMessage(MessageModel message)
    {
        await _messagesDbContext.AddMessageAsync(message);
    }

    internal async Task UploadImageAsync(ImageModel imageModel)
    {
        await _messagesDbContext.UploadImageAsync(imageModel);
    }

    internal ImageModel GetImage(Guid imageId)
    {
        return _messagesDbContext.Images.FirstOrDefault(m => m.Id==imageId);
    }
}
