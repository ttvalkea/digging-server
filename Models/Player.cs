public class Player: ItemBase
{
    public string playerName { get; set; }
    public string playerColor { get; set; }
    public int hitPoints { get; set; }
    public Enums.MovementState movementState { get; set; }
    public int movementIntervalMs { get; set; }
    public int score { get; set; }
}