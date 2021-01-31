using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent : Entity
{
    public PathWeb pathWeb;

    private const float NODE_SATISFACTION = .5f;
    private const float NODE_SATISFACTION_SQR = NODE_SATISFACTION*NODE_SATISFACTION;

    public Vector2 WaypointToTarget(Vector2 target)
    {
        var path = pathWeb.PathTo(transform.position, target);
        
        if (path == null) return transform.position;

        Debug.DrawLine(transform.position, path[0], Color.magenta);
        path.zipWithNext((a, b) => Debug.DrawLine(a, b, Color.magenta) );

        if (path.Count == 1) return path[0];

        var asdf = (transform.position2() - target).sqrMagnitude;

        if ((transform.position2() - path[0]).sqrMagnitude < NODE_SATISFACTION_SQR)
            return path[1];


        return path[0];
    }
}
