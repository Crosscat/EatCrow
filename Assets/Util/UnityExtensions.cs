using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    public static Vector2 position2(this Transform transform)
    {
        return (Vector2) transform.position;
    }

    public static Vector2 bottomLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }

    public static Vector2 bottomRight(this Rect rect)
    {
        return new Vector2(rect.xMax, rect.yMin);
    }

    public static Vector2 topLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMax);
    }

    public static Vector2 topRight(this Rect rect)
    {
        return new Vector2(rect.xMax, rect.yMax);
    }
}
