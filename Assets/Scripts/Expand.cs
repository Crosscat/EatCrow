using UnityEngine;

public class Expand : MonoBehaviour
{
    public float ExpandSpeed;

    void Update()
    {
        var scale = transform.localScale;
        var adder = Vector3.one * ExpandSpeed * Time.deltaTime;
        adder.x *= Mathf.Sign(scale.x);
        scale += adder;
        transform.localScale = scale;
    }
}
