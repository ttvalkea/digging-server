public class NewTagItem : ItemBase
{
    public bool isInPlay { get; set; }

    public NewTagItem(int x, int y, bool inPlay)
    {
        positionX = x;
        positionY = y;
        id = Utils.GetId();
        sizeX = Constants.NEW_TAG_ITEM_SIZE_X;
        sizeY = Constants.NEW_TAG_ITEM_SIZE_Y;
        isInPlay = inPlay;
    }
}