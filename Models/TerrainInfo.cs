using static Enums;

public class TerrainInfo
{
    public TerrainType terrainType { get; set; }
    public Coordinate coordinate { get; set; }
    public TerrainInfo(Coordinate c, TerrainType tt)
    {
        coordinate = c;
        terrainType = tt;
    }
}