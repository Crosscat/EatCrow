using UnityEngine;

public class Fade : MonoBehaviour
{
    public float FadeAcceleration;
    public float FadeRate;
    public float Visibility => _color.a;

    private SpriteRenderer _sprite;
    private Color _color;

    public void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        _color = _sprite.color;
        FadeRate += FadeAcceleration * Time.deltaTime;
        _color.a = Mathf.Clamp(_color.a - FadeRate * Time.deltaTime, 0, 1);
        _sprite.color = _color;
    }
}
