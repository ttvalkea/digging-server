using System.Collections.Generic;

public class MapInfo
{
    public List<Coordinate> emptySpaces { get; set; }
    public List<Obstacle> obstacles { get; set; }
    public List<SoilInfo> soilTiles { get; set; }
}