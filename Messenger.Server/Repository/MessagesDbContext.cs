using Messenger.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Messenger.Server.Repository;

public class MessagesDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(databaseName: "Messages");
        base.OnConfiguring(optionsBuilder);
    }
    public MessagesDbContext(DbContextOptions<MessagesDbContext> options)
            : base(options)
    {
    }

    public DbSet<MessageModel> Messages { get; set; }
    public async Task AddMessageAsync(MessageModel currencyRateDbModel)
    {
        await Messages.AddAsync(currencyRateDbModel);
        await SaveChangesAsync();
    }


}
