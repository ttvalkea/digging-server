public class NewTagItem : ItemBase
{
    public bool isInPlay { get; set; }

    public NewTagItem(int x, int y, bool inPlay)
    {
        positionX = x;
        positionY = y;
        id = Utils.GetId();
        isInPlay = inPlay;
    }
}