using System;
using System.Collections.Generic;
using UnityEngine;

public class RaycastLauncher : MonoBehaviour
{
    public List<Transform> IgnoredObjects = new List<Transform>();
    public List<RaycastTarget> RaycastTargets = new List<RaycastTarget>();

    public Collider2D Collider;
    public float NumOfRaycastsWidth = 3;

    public List<RaycastHit2D> GetHitData(Vector2 direction, float rayDistance, float margin)
    {
        var points = GetRaycastOrigins(direction, margin);
        var hits = new List<RaycastHit2D>();

        foreach (var target in RaycastTargets)
        {
            if (target.IgnoredDirections.Contains(direction.normalized)) continue;

            foreach (var point in points)
            {
                var hitData = Physics2D.Raycast(point, direction.normalized, rayDistance, target.Mask);
                if (IgnoredObjects.Contains(hitData.transform)) continue;
                hits.Add(hitData);
            }
        }
        
        return hits;
    }

    public List<Vector2> GetRaycastOrigins(Vector2 direction, float margin)
    {
        if (direction == Vector2.zero)
            return new List<Vector2>();

        var start = new Vector2(Collider.bounds.min.x + margin, Collider.bounds.min.y + margin);
        var end = new Vector2(Collider.bounds.max.x - margin, Collider.bounds.max.y - margin);

        var iTarget = new Vector2();

        if (direction.x != 0)
        {
            start.x = Collider.bounds.center.x;
            end.x = Collider.bounds.center.x;
            iTarget = new Vector2(start.x, start.y);
        }
        else if (direction.y != 0)
        {
            start.y = Collider.bounds.center.y;
            end.y = Collider.bounds.center.y;
            iTarget = new Vector2(end.x, start.y);
        }

        var points = new List<Vector2>();

        for (int i = 0; i < NumOfRaycastsWidth; i++)
        {
            var iLerp = (float)i / (NumOfRaycastsWidth - 1);
            var iVector = Vector2.Lerp(start, iTarget, iLerp);
            var jTarget = GetJTarget(direction, iVector, end);

            //for (var j = 0; j < NumOfRaycastsLength; j++)
            //{
            //    //int rayIndex = j * _numOfRaycastsWidth + i;

            //    var jLerp = (float)j / (NumOfRaycastsLength - 1);
            //    var jVector = Vector3.Lerp(iVector, jTarget, jLerp);

                points.Add(iVector);
            //}
        }

        return points;
    }

    private Vector3 GetJTarget(Vector3 direction, Vector3 iVector, Vector3 endVector)
    {
        var targetVector = new Vector3(iVector.x, iVector.y, iVector.z);
        if (direction.x != 0)
        {
            targetVector.y = endVector.y;
        }
        else if (direction.y != 0)
        {
            targetVector.z = endVector.z;
        }
        else if (direction.z != 0)
        {
            targetVector.x = endVector.x;
        }

        return targetVector;
    }
}

[Serializable]
public struct RaycastTarget
{
    public LayerMask Mask;
    public List<Vector2> IgnoredDirections;
}
