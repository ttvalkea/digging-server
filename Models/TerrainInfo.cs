using static Enums;

public class TerrainInfo : IHasPosition
{
    public int positionX { get; set; }
    public int positionY { get; set; }
    public TerrainType terrainType { get; set; }
    public TerrainInfo(int x, int y, TerrainType tt)
    {
        positionX = x;
        positionY = y;
        terrainType = tt;
    }
}