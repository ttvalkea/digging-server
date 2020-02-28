using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

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
    public async Task BroadcastGetObstacles(bool generateNewObstacles) => await Clients.All.SendAsync("broadcastGetObstacles", GetObstacles(generateNewObstacles));

    public async Task BroadcastNewTagItemData() => await Clients.All.SendAsync("newTag", PersistingValues.TagItem);

    public async Task BroadcastDigMessage(int positionX, int positionY) => await Clients.All.SendAsync("broadcastDigMessage", GetDigResponse(positionX, positionY));

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

    public List<Obstacle> GetObstacles(bool generateNewObstacles)
    {
        if (generateNewObstacles)
        {
            var rng = new Random();
            var amount = rng.Next(Constants.OBSTACLE_AMOUNT_MIN, Constants.OBSTACLE_AMOUNT_MAX);
            var obstacles = new List<Obstacle>();
            for (var i = 0; i < amount; i++)
            {
                var newObstacle = new Obstacle(rng.Next(Constants.OBSTACLE_POSITION_X_MIN, Constants.OBSTACLE_POSITION_X_MAX), rng.Next(Constants.OBSTACLE_POSITION_Y_MIN, Constants.OBSTACLE_POSITION_Y_MAX));

                //Don't allow obstacles with the same position.
                if (obstacles.Any(o => o.positionX == newObstacle.positionX && o.positionY == newObstacle.positionY))
                {
                    i--;
                }
                else
                {
                    if (PersistingValues.EmptySpaces.Any(emptySpace => emptySpace.x == newObstacle.positionX && emptySpace.y == newObstacle.positionY))
                    {
                        newObstacle.isVisible = true;
                    }
                    obstacles.Add(newObstacle);
                }
            }
            PersistingValues.Obstacles = obstacles;
        }

        return PersistingValues.Obstacles;
    }

    public async Task BroadcastGetEmptySpaces() => await Clients.All.SendAsync("broadcastGetEmptySpaces", PersistingValues.EmptySpaces);

    private TerrainInfo GetDigResponse(int positionX, int positionY)
    {
        var newPosition = new Coordinate(positionX, positionY);
        var terrainType = Enums.TerrainType.Empty;

        //If the position that is being dug has an obstacle, don't make an empty space 
        var possibleObstacle = PersistingValues.Obstacles.FirstOrDefault(obstacle => obstacle.positionX == newPosition.x && obstacle.positionY == newPosition.y);
        if (possibleObstacle != null)
        {
            possibleObstacle.isVisible = true;
            terrainType = Enums.TerrainType.Obstacle;
        }      
        else if (!PersistingValues.EmptySpaces.Any(coordinate => coordinate.x == newPosition.x && coordinate.y == newPosition.y))
        {
            PersistingValues.EmptySpaces.Add(newPosition);
        }
        
        return new TerrainInfo(newPosition, terrainType);
    }
}
