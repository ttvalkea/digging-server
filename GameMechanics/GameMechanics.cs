
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

            var emptyTiles = new List<Coordinate>();
            //Uncomment for an empty map
            //for (int x = 0; x < mapSizeX; x++)
            //{
            //    for (int y = 0; y < mapSizeY; y++)
            //    {
            //        if (!PersistingValues.Obstacles.Any(o => o.positionX == x && o.positionY == y)) {
            //            emptyTiles.Add(new Coordinate(x, y));
            //        }
            //    }
            //}
            PersistingValues.EmptySpaces = emptyTiles;
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

        //Soil tiles are generated in phases
        var soilGenerationPhases = new List<SoilGenerationData>()
        {
            new SoilGenerationData()
            {
                areaDiameterPercentageOfMapSize = Constants.BEST_SOIL_AREA_DIAMETER_PERCENTAGE_OF_MAP_SIZE,
                areaPercentageOfAllSoilTiles = Constants.BEST_SOIL_AREA_PERCENTAGE_OF_ALL_SOIL_TILES,
                areaOddsForLevel1Soil = Constants.BEST_SOIL_AREA_ODDS_FOR_LEVEL_1_SOIL,
                areaOddsForLevel2Soil = Constants.BEST_SOIL_AREA_ODDS_FOR_LEVEL_2_SOIL,
                areaOddsForLevel3Soil = Constants.BEST_SOIL_AREA_ODDS_FOR_LEVEL_3_SOIL
            },
            new SoilGenerationData()
            {
                areaDiameterPercentageOfMapSize = Constants.SECOND_BEST_SOIL_AREA_DIAMETER_PERCENTAGE_OF_MAP_SIZE,
                areaPercentageOfAllSoilTiles = Constants.SECOND_BEST_SOIL_AREA_PERCENTAGE_OF_ALL_SOIL_TILES,
                areaOddsForLevel1Soil = Constants.SECOND_BEST_SOIL_AREA_ODDS_FOR_LEVEL_1_SOIL,
                areaOddsForLevel2Soil = Constants.SECOND_BEST_SOIL_AREA_ODDS_FOR_LEVEL_2_SOIL,
                areaOddsForLevel3Soil = Constants.SECOND_BEST_SOIL_AREA_ODDS_FOR_LEVEL_3_SOIL
            },
            new SoilGenerationData()
            {
                areaDiameterPercentageOfMapSize = Constants.THIRD_BEST_SOIL_AREA_DIAMETER_PERCENTAGE_OF_MAP_SIZE,
                areaPercentageOfAllSoilTiles = Constants.THIRD_BEST_SOIL_AREA_PERCENTAGE_OF_ALL_SOIL_TILES,
                areaOddsForLevel1Soil = Constants.THIRD_BEST_SOIL_AREA_ODDS_FOR_LEVEL_1_SOIL,
                areaOddsForLevel2Soil = Constants.THIRD_BEST_SOIL_AREA_ODDS_FOR_LEVEL_2_SOIL,
                areaOddsForLevel3Soil = Constants.THIRD_BEST_SOIL_AREA_ODDS_FOR_LEVEL_3_SOIL
            }
        };

        var rng = new Random();
        var soilTiles = new List<SoilInfo>();
        var totalSoilTileAmount = rng.Next(soilAmountMin, soilAmountMax);
        foreach (var soilGenerationPhase in soilGenerationPhases)
        {
            var soilTileAmount = (int)Math.Round((double)totalSoilTileAmount * soilGenerationPhase.areaPercentageOfAllSoilTiles / 100);

            var minXCoordinate = GetAreaOffsetFromPlayAreaEdgeForSoilGeneration(mapSizeX, soilGenerationPhase.areaDiameterPercentageOfMapSize);
            var maxXCoordinate = mapSizeX - GetAreaOffsetFromPlayAreaEdgeForSoilGeneration(mapSizeX, soilGenerationPhase.areaDiameterPercentageOfMapSize);
            var minYCoordinate = GetAreaOffsetFromPlayAreaEdgeForSoilGeneration(mapSizeY, soilGenerationPhase.areaDiameterPercentageOfMapSize);
            var maxYCoordinate = mapSizeY - GetAreaOffsetFromPlayAreaEdgeForSoilGeneration(mapSizeY, soilGenerationPhase.areaDiameterPercentageOfMapSize);

            // Soil level pool has shares of different soil levels. 
            var soilLevelPool = GetSoilLevelPool(soilGenerationPhase);

            for (var i = 0; i < soilTileAmount; i++)
            {
                var coordinate = new Coordinate(rng.Next(minXCoordinate, maxXCoordinate), rng.Next(minYCoordinate, maxYCoordinate));
                var soilLevel = soilLevelPool[rng.Next(soilLevelPool.Count)];
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
        }        
        
        return soilTiles;
    }

    private static List<SoilLevel> GetSoilLevelPool(SoilGenerationData soilGenerationPhase)
    {
        var soilLevelPool = new List<SoilLevel>();
        for (int i = 0; i < soilGenerationPhase.areaOddsForLevel1Soil; i++)
        {
            soilLevelPool.Add(SoilLevel.Good);
        }
        for (int i = 0; i < soilGenerationPhase.areaOddsForLevel2Soil; i++)
        {
            soilLevelPool.Add(SoilLevel.Great);
        }
        for (int i = 0; i < soilGenerationPhase.areaOddsForLevel3Soil; i++)
        {
            soilLevelPool.Add(SoilLevel.Exuberant);
        }
        return soilLevelPool;
    }

    private static int GetAreaOffsetFromPlayAreaEdgeForSoilGeneration(int mapSize, int areaDiameterPercentageOfMapSize)
    {
        var offsetFromTheEdge = (int)Math.Round((double)mapSize / 2) - (int)Math.Round((double)mapSize * areaDiameterPercentageOfMapSize / 100 / 2);
        return offsetFromTheEdge;
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


