public class Obstacle : IHasPosition
{
    public int positionX { get; set; }
    public int positionY { get; set; }
    public bool isVisible { get; set; }
    public Obstacle(int x, int y)
    {
        positionX = x;
        positionY = y;
    }
}