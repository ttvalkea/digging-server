public class Coordinate : IHasPosition
{
    public int positionX { get; set; }
    public int positionY { get; set; }

    public Coordinate(int x, int y)
    {
        positionX = x;
        positionY = y;
    } 
}