using Messenger.Server.Repository;
using Messenger.Server.Services;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Messenger.Server;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSignalR();
        builder.Services.AddControllers();
        var optionsBuilder = new DbContextOptionsBuilder<MessagesDbContext>();
        optionsBuilder.UseInMemoryDatabase("Messages");
        builder.Services.AddSingleton(new ChatHub(optionsBuilder.Options));
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<MessagesDbContext>();
        builder.Services.AddScoped<MessagesRepository>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseRouting();

        app.MapHub<ChatHub>("/chatHub");

        app.Run();
    }
}
