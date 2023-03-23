/// <summary>
/// 元素類型
/// </summary>
public enum ElementType 
{
    None,
    Red,
    Yellow,
    Green,
    Blue,
    Purple,
    White
}

public enum Direction
{
    UP,
    DOWN,
    RIGHT,
    LEFT
}

public enum PlayingStatus
{
    Waiting,
    // Operating -> 操作中
    Operating,
    Moving,
    // Matching -> 匹配中
    Matching,
    ReadyToDrop,
    Droping
}
