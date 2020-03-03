
using System;
using System.Collections.Generic;
using System.Linq;
using static Enums;

public static class GameMechanics
{
    public static MapInfo GetMapInfo(bool generateNewMap, int? mapSizeX = null, int? mapSizeY = null, int? obstacleAmountMin = null, int? obstacleAmountMax = null, int? soilAmountMin = null, int? soilAmountMax = null)
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

    public static List<SoilInfo> GenerateSoilTiles(int mapSizeX, int mapSizeY, int soilAmountMin, int soilAmountMax)
    {
        //Obstacles should be generated before generating soil tiles!

        var rng = new Random();
        var amount = rng.Next(soilAmountMin, soilAmountMax);
        var soilTiles = new List<SoilInfo>();
        for (var i = 0; i < amount; i++)
        {
            var coordinate = new Coordinate(rng.Next(0, mapSizeX), rng.Next(0, mapSizeY));
            var soilLevel = (SoilLevel)(rng.Next(1, 4)); // Returns soil level 1-3
            var newSoilTile = new SoilInfo(coordinate.positionX, coordinate.positionY, soilLevel);
            //Don't allow obstacles or other soil tiles with the same position.
            if (soilTiles.Any(st => st.positionX == newSoilTile.positionX && st.positionY == newSoilTile.positionY)
                || PersistingValues.Obstacles.Any(o => o.positionX == newSoilTile.positionX && o.positionY == newSoilTile.positionY))
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

    public static List<Obstacle> GenerateObstacles(int mapSizeX, int mapSizeY, int obstacleAmountMin, int obstacleAmountMax)
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
                if (PersistingValues.EmptySpaces.Any(emptySpace => emptySpace.positionX == newObstacle.positionX && emptySpace.positionY == newObstacle.positionY))
                {
                    newObstacle.isVisible = true;
                }
                obstacles.Add(newObstacle);
            }
        }
        return obstacles;
    }

    public static TerrainInfo GetDigResponse(int positionX, int positionY)
    {
        var newPosition = new Coordinate(positionX, positionY);
        var terrainType = Enums.TerrainType.Empty;

        //If the position that is being dug has an obstacle, don't make an empty space 
        var possibleObstacle = PersistingValues.Obstacles.FirstOrDefault(obstacle => obstacle.positionX == newPosition.positionX && obstacle.positionY == newPosition.positionY);
        if (possibleObstacle != null)
        {
            possibleObstacle.isVisible = true;
            terrainType = Enums.TerrainType.Obstacle;
        }
        else if (!PersistingValues.EmptySpaces.Any(coordinate => coordinate.positionX == newPosition.positionX && coordinate.positionY == newPosition.positionY))
        {
            PersistingValues.EmptySpaces.Add(newPosition);
        }

        return new TerrainInfo(newPosition.positionX, newPosition.positionY, terrainType);
    }

    // Returns the tiles where new fruits were grown.
    public static List<SoilInfo> GenerateNewFruits()
    {
        var rng = new Random();
        var soilTilesWithNewFruit = new List<SoilInfo>();
        foreach (var soilTile in PersistingValues.SoilTiles)
        {
            //Soil tile is revealed and doesn't already have fruit
            if (PersistingValues.EmptySpaces.Any(emptySpace => emptySpace.positionX == soilTile.positionX && emptySpace.positionY == soilTile.positionY) && !soilTile.hasFruit)
            {
                if (rng.Next(1, 1000) <= Constants.FRUIT_GROWTH_CHANCE_PERMILLE)
                {
                    soilTile.hasFruit = true;
                    soilTilesWithNewFruit.Add(soilTile);
                }
            }
        }
        return soilTilesWithNewFruit;
    }

    public static SoilInfo GatherFruitFromTile(int positionX, int positionY)
    {
        var soilTile = PersistingValues.SoilTiles.First(tile => tile.positionX == positionX && tile.positionY == positionY);
        soilTile.hasFruit = false;
        return soilTile;
    }

    public static int GetPointAmountFromFruit(SoilLevel fruitLevel)
    {
        switch (fruitLevel)
        {
            case SoilLevel.Good:
                return Constants.LEVEL_1_FRUIT_POINTS;
            case SoilLevel.Great:
                return Constants.LEVEL_2_FRUIT_POINTS;
            case SoilLevel.Exuberant:
                return Constants.LEVEL_3_FRUIT_POINTS;
        }
        throw new Exception("Invalid SoilLevel");
    }
}


