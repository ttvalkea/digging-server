using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DiggingHub : Hub
{
    public async Task BroadcastPlayerWins(Player player) => await Clients.All.SendAsync("broadcastPlayerWins", player);
    public async Task BroadcastConnectionAmountData(int data) => await Clients.All.SendAsync("broadcastconnectionamountdata", data);
    public async Task BroadcastPlayerDataMessage(Player data) => await Clients.All.SendAsync("broadcastPlayerDataMessage", data);
    public async Task BroadcastFireballDataMessage(Fireball data) => await Clients.All.SendAsync("broadcastFireballDataMessage", data);
    public async Task BroadcastFireballHitPlayerMessage(Fireball fireball, Player player)
    {
        await Clients.All.SendAsync("broadcastFireballHitPlayerMessage", new FireballHitPlayerData(fireball, player));
    }

    public async Task BroadcastDigMessage(int positionX, int positionY) => await Clients.All.SendAsync("broadcastDigMessage", GameMechanics.GetDigResponse(positionX, positionY));

    public async Task BroadcastMapInfo(bool generateNewMap, int? mapSizeX = null, int? mapSizeY = null, int? obstacleAmountMin = null, int? obstacleAmountMax = null, int? soilAmountMin = null, int? soilAmountMax = null) 
        => await Clients.All.SendAsync("broadcastMapInfo", GameMechanics.GetMapInfo(generateNewMap, mapSizeX, mapSizeY, obstacleAmountMin, obstacleAmountMax, soilAmountMin, soilAmountMax));

    public async Task BroadcastPlayerHitFruit(int positionX, int positionY, string playerId)
    {
        var tileFromWhichFruitWasGathered = GameMechanics.GatherFruitFromTile(positionX, positionY);
        await Clients.All.SendAsync("fruitInfo", new List<SoilInfo>() { tileFromWhichFruitWasGathered });
        await Clients.All.SendAsync("playerGotPoints", new PlayerGotPoints(playerId, GameMechanics.GetPointAmountFromFruit(tileFromWhichFruitWasGathered.soilLevel)));
    }
    
    public async Task BroadcastGetEmptySpaces() => await Clients.All.SendAsync("broadcastGetEmptySpaces", PersistingValues.EmptySpaces);

    public override Task OnConnectedAsync()
    {
        PersistingValues.IdsOfConnectedClients.Add(Context.ConnectionId);
        BroadcastConnectionAmountData(PersistingValues.IdsOfConnectedClients.Count);

        return base.OnConnectedAsync();
    }
    public override Task OnDisconnectedAsync(Exception exception)
    {
        PersistingValues.IdsOfConnectedClients.Remove(Context.ConnectionId);
        BroadcastConnectionAmountData(PersistingValues.IdsOfConnectedClients.Count);
        return base.OnDisconnectedAsync(exception);
    }    
}
