using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AIZone : MonoBehaviour
{
    public bool walkable;

    private RectTransform rectTransform;

    public Rect Rect
    {
        get { return new Rect(transform.position2() - rectTransform.rect.size/2, rectTransform.rect.size); }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 1, .1f);
        Gizmos.DrawCube(transform.position, GetComponent<RectTransform>().rect.size);
    }
}
