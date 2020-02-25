using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Timers;

[Route("api/[controller]")]
[ApiController]
public class HubController : ControllerBase
{
    private IHubContext<DiggingHub> _hub;

    public HubController(IHubContext<DiggingHub> hub)
    {
        _hub = hub;
        SetTimer();
    }

    public IActionResult Get()
    {
        return Ok(new { Message = "Request Completed" });
    }

    //Set up a loop
    private void SetTimer()
    {
        PersistingValues.NewTagItemTimer.Elapsed += OnNewTagItemTimerEvent;
    }

    //Send data to all clients periodically
    private async void OnNewTagItemTimerEvent(Object source, ElapsedEventArgs e)
    {
        //Only sent once every ten seconds no matter the number of clients.
        if (PersistingValues.IdsOfConnectedClients.Count > 0 && !PersistingValues.NewTagInfoSentThisCycle)
        {
            PersistingValues.NewTagInfoSentThisCycle = true;
            await SendNewTagPositionToAllClients();
        }
    }

    private async Task SendNewTagPositionToAllClients()
    {
        var rng = new Random();
        var x = rng.Next(1, 74);
        var y = rng.Next(1, 74);
        PersistingValues.TagItem = new NewTagItem(x, y, true);
        await _hub.Clients.All.SendAsync("newTag", PersistingValues.TagItem); 
    }

    public Timer GameTimer { get; set; }
}