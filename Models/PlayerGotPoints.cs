public class PlayerGotPoints
{
    public string playerId { get; set; }
    public int amount { get; set; }

    public PlayerGotPoints(string id, int pointAmount)
    {
        playerId = id;
        amount = pointAmount;
    }
}