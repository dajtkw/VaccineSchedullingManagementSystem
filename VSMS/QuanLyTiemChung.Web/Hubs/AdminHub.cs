// Hubs/AdminHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class AdminHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Log user connection
        var userId = Context.UserIdentifier;
        Console.WriteLine($"User {userId} connected to SignalR");
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"User {userId} disconnected from SignalR");
        
        await base.OnDisconnectedAsync(exception);
    }
}
