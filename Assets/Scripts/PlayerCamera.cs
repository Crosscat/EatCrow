using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    public Rect viewableRegion = new Rect(-20, -20, 40, 40);
    
    new private Camera camera;
    private Transform playerTransform;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Start()
    {
        playerTransform = GameObject.FindObjectOfType<PlayerController>().transform;
    }

    private void LateUpdate()
    {
        Vector2 extents = new Vector2(camera.orthographicSize * Screen.width / Screen.height, camera.orthographicSize);

        Vector3 target = new Vector3(
            Mathf.Clamp(playerTransform.position.x, viewableRegion.xMin + extents.x, viewableRegion.xMax - extents.x),
            Mathf.Clamp(playerTransform.position.y, viewableRegion.yMin + extents.y, viewableRegion.yMax - extents.y),
            -10
        );

        transform.position = Vector3.Lerp(transform.position, target, 10 * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(viewableRegion.center, viewableRegion.size);
    }
}
