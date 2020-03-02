public class Player : IHasPosition
{
    public int positionX { get; set; }
    public int positionY { get; set; }
    public string playerName { get; set; }
    public string playerColor { get; set; }
    public int hitPoints { get; set; }
    public int score { get; set; }
    public string id { get; set; }
    public decimal direction { get; set; }
}