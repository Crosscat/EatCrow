using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathSegment: MonoBehaviour
{
    public PathSegmentType type;
    public PathNode From;
    public PathNode To;
    public bool OneWay = false;

    private const int INTERPOLATION_FACTOR = 10;
    private static readonly GUIStyle handleStyle = new GUIStyle();

    public Vector2 FromPosition
    {
        get { return From.transform.position; }
    }

    public Vector2 ToPosition
    {
        get { return To.transform.position; }
    }

    public Vector2 Delta
    {
        get { return ToPosition - FromPosition; }
    }

    public float Length
    {
        get { return Delta.magnitude; }
    }

    private List<Vector2> _interpolationPoints; 
    public List<Vector2> InterpolationPoints
    {
        get
        {
            if (_interpolationPoints == null)
            {
                _interpolationPoints = new List<Vector2>() { FromPosition, ToPosition };

                if (type == PathSegmentType.Walkable)
                {
                    Vector2 interpolationDelta = (ToPosition - FromPosition) / INTERPOLATION_FACTOR;
                    for (int i=1; i<INTERPOLATION_FACTOR; i++)
                    {
                        _interpolationPoints.Add(FromPosition + interpolationDelta * i);
                    }
                }
            }
            return _interpolationPoints;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white/2;
        DrawGizmos();
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        if (From != null && To != null)
        {
            Gizmos.DrawLine(FromPosition, ToPosition);
            Vector2 direction = (ToPosition - FromPosition).normalized;

            Vector2 arrowLeft = Quaternion.Euler(0, 0, -135) * direction * .4f;
            Vector2 arrowRight = Quaternion.Euler(0, 0, 135) * direction * .4f;

            Gizmos.DrawLine(ToPosition, ToPosition + arrowLeft);
            Gizmos.DrawLine(ToPosition, ToPosition + arrowRight);
            if (!OneWay)
            {
                Gizmos.DrawLine(FromPosition, FromPosition - arrowLeft);
                Gizmos.DrawLine(FromPosition, FromPosition - arrowRight);
            }

            handleStyle.normal.textColor = Gizmos.color;
            UnityEditor.Handles.Label(FromPosition + (ToPosition - FromPosition) / 2, gameObject.name, handleStyle);
        }
    }
}

public enum PathSegmentType
{
    Walkable,
    Jumpable,
    Fallable
}
