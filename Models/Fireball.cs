public class Fireball: IHasPosition
{
    public int positionX { get; set; }
    public int positionY { get; set; }
    public string casterId { get; set; }
    public int moveIntervalMs { get; set; }
    public bool isDestroyed { get; set; }
    public string id { get; set; }
    public decimal direction { get; set; }

}