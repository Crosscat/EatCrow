using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    public static Vector2 position2(this Transform transform)
    {
        return (Vector2) transform.position;
    }
}
