using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Enums;

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

    public async Task BroadcastDigMessage(int positionX, int positionY) => await Clients.All.SendAsync("broadcastDigMessage", GetDigResponse(positionX, positionY));

    public async Task BroadcastMapInfo(bool generateNewMap, int? mapSizeX = null, int? mapSizeY = null, int? obstacleAmountMin = null, int? obstacleAmountMax = null, int? soilAmountMin = null, int? soilAmountMax = null) 
        => await Clients.All.SendAsync("broadcastMapInfo", GetMapInfo(generateNewMap, mapSizeX, mapSizeY, obstacleAmountMin, obstacleAmountMax, soilAmountMin, soilAmountMax));


    public override Task OnConnectedAsync()
    {
        PersistingValues.IdsOfConnectedClients.Add(Context.ConnectionId);
        BroadcastConnectionAmountData(PersistingValues.IdsOfConnectedClients.Count);

        return base.OnConnectedAsync();
    }

    private MapInfo GetMapInfo(bool generateNewMap, int? mapSizeX = null, int? mapSizeY = null, int? obstacleAmountMin = null, int? obstacleAmountMax = null, int? soilAmountMin = null, int? soilAmountMax = null)
    {
        if (generateNewMap && (mapSizeX == null || mapSizeY == null || obstacleAmountMin == null || obstacleAmountMax == null || soilAmountMin == null || soilAmountMax == null))
        {
            throw new Exception("When generating a new map (generateNewMap == true), all the other parameters must be given (mapSizeY, mapSizeX, obstacleAmountMin, obstacleAmountMax, soilAmountMin, soilAmountMax)");
        }

        if (generateNewMap)
        {
            PersistingValues.Obstacles = GenerateObstacles((int)mapSizeX, (int)mapSizeY, (int)obstacleAmountMin, (int)obstacleAmountMax);
            PersistingValues.SoilTiles = GenerateSoilTiles((int)mapSizeX, (int)mapSizeY, (int)soilAmountMin, (int)soilAmountMax);
            PersistingValues.EmptySpaces = new List<Coordinate>();            
        }
        return new MapInfo()
        {
            emptySpaces = PersistingValues.EmptySpaces,
            obstacles = PersistingValues.Obstacles,
            soilTiles = PersistingValues.SoilTiles
        };
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        PersistingValues.IdsOfConnectedClients.Remove(Context.ConnectionId);
        BroadcastConnectionAmountData(PersistingValues.IdsOfConnectedClients.Count);
        return base.OnDisconnectedAsync(exception);
    }

    private List<SoilInfo> GenerateSoilTiles(int mapSizeX, int mapSizeY, int soilAmountMin, int soilAmountMax)
    {
        //Obstacles should be generated before generating soil tiles!

        var rng = new Random();
        var amount = rng.Next(soilAmountMin, soilAmountMax);
        var soilTiles = new List<SoilInfo>();
        for (var i = 0; i < amount; i++)
        {
            var coordinate = new Coordinate(rng.Next(0, mapSizeX), rng.Next(0, mapSizeY));
            var soilLevel = (SoilLevel)(rng.Next(1, 4)); // Returns soil level 1-3
            var newSoilTile = new SoilInfo(coordinate, soilLevel);
            //Don't allow obstacles or other soil tiles with the same position.
            if (soilTiles.Any(st => st.coordinate.x == newSoilTile.coordinate.x && st.coordinate.y == newSoilTile.coordinate.y) 
                || PersistingValues.Obstacles.Any(o => o.positionX == newSoilTile.coordinate.x && o.positionY == newSoilTile.coordinate.y))
            {
                i--;
            }
            else
            {
                soilTiles.Add(newSoilTile);
            }
        }
        return soilTiles;
    }

    private List<Obstacle> GenerateObstacles(int mapSizeX, int mapSizeY, int obstacleAmountMin, int obstacleAmountMax)
    {
        var rng = new Random();
        var amount = rng.Next(obstacleAmountMin, obstacleAmountMax);
        var obstacles = new List<Obstacle>();
        for (var i = 0; i < amount; i++)
        {
            var newObstacle = new Obstacle(rng.Next(0, mapSizeX), rng.Next(0, mapSizeY));

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
        return obstacles;
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
