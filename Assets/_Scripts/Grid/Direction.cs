using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    Current,
}

public static class DirectionHelper
{
    public static Vector2Int GetNeighbor(Vector2Int currentPos, Direction direction)
    {
        Vector2Int result = direction switch
        {
            Direction.Up => currentPos + Vector2Int.up,
            Direction.Down => currentPos + Vector2Int.down,
            Direction.Left => currentPos + Vector2Int.left,
            Direction.Right => currentPos + Vector2Int.right,
            Direction.Current => currentPos,
            _ => currentPos
        };
        return result;
    }
}