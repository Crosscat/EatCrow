using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FloorTiler : MonoBehaviour
{
    public List<GameObject> FloorTiles;
    public float Spacing = .5f;

    private void Awake()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        Rect rect = new Rect(box.bounds.min, box.bounds.size);

        GameObject group = new GameObject();

        for (float x = rect.xMin + Spacing/2; x <= rect.xMax - Spacing/2; x += Spacing)
        {
            for (float y = rect.yMin + Spacing/2; y <= rect.yMax - Spacing/2; y += Spacing)
            {
                GameObject tile = GameObject.Instantiate
                (
                    FloorTiles.PickRandom(), 
                    new Vector2(x, y), 
                    Quaternion.Euler(0,0,Random.Range(0,359.9f)), 
                    group.transform
                );
                tile.GetComponent<SpriteRenderer>().sortingOrder = Random.Range(1, 10);
            }
        }

        GameObject.Destroy(this);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(.7f, 0, .7f, 1f);
        Gizmos.DrawCube(transform.position, GetComponent<BoxCollider2D>().bounds.size);
    }
}
