using static Enums;

public class SoilInfo
{
    public SoilLevel soilLevel { get; set; }
    public Coordinate coordinate { get; set; }
    public bool hasFruit { get; set; }
    public SoilInfo(Coordinate c, SoilLevel sl, bool hf = false)
    {
        coordinate = c;
        soilLevel = sl;
        hasFruit = hf;
    }
}