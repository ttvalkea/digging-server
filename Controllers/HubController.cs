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
        PersistingValues.FruitInfoTimer.Elapsed += OnFruitInfoTimerEvent;
    }

    //Send data to all clients periodically
    private async void OnFruitInfoTimerEvent(Object source, ElapsedEventArgs e)
    {
        //Only sent once every ten seconds no matter the number of clients.
        if (PersistingValues.IdsOfConnectedClients.Count > 0 && !PersistingValues.FruitInfoSentThisCycle)
        {
            PersistingValues.FruitInfoSentThisCycle = true;
            await SendFruitInfoToAllClients();
        }
    }

    private async Task SendFruitInfoToAllClients()
    {
        var tilesWithNewFruits = GameMechanics.GenerateNewFruits();
        await _hub.Clients.All.SendAsync("fruitInfo", tilesWithNewFruits); 
    }    

    public Timer GameTimer { get; set; }
}