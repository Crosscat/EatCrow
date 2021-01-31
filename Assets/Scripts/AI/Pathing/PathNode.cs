using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathNode : MonoBehaviour
{
    private static readonly GUIStyle handleStyle = new GUIStyle();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, .1f);

        handleStyle.normal.textColor = Gizmos.color;
        Handles.Label(transform.position, gameObject.name, handleStyle);
    }
}
