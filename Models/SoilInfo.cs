using static Enums;

public class SoilInfo : IHasPosition
{
    public int positionX { get; set; }
    public int positionY { get; set; }
    public SoilLevel soilLevel { get; set; }
    public bool hasFruit { get; set; }
    public SoilInfo(int x, int y, SoilLevel sl, bool hf = false)
    {
        positionX = x;
        positionY = y;
        soilLevel = sl;
        hasFruit = hf;
    }
}