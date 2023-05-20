using Messenger.Server.Services;

using Microsoft.AspNetCore.OpenApi;

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
        builder.Services.AddSingleton<ChatHub>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.


        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseRouting();

        app.MapHub<ChatHub>("/chatHub");

        app.Run();
    }
}
