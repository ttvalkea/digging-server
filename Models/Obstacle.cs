public class Obstacle: ItemBase
{
    public bool isVisible { get; set; }
    public Obstacle(int x, int y)
    {
        positionX = x;
        positionY = y;
    }
}