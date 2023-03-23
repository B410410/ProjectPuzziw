/// <summary>
/// ��������
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
    // Operating -> �ާ@��
    Operating,
    Moving,
    // Matching -> �ǰt��
    Matching,
    ReadyToDrop,
    Droping
}
