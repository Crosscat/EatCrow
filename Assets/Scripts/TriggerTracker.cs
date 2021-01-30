using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TriggerTracker<T> : MonoBehaviour where T:MonoBehaviour
{
    public readonly List<T> triggered = new List<T>();

    private void OnTriggerEnter2D(Collider2D col)
    {
        T item = col.GetComponent<T>();
        if (item != null)
        {
            triggered.Add(item);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        T item = col.GetComponent<T>();
        if (item != null)
        {
            triggered.Remove(item);
        }
    }
}