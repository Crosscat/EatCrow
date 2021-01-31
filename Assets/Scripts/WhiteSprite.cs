using UnityEngine;

public class WhiteSprite : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Shader _shaderGUItext;

    void Awake()
    {
        _renderer = gameObject.GetComponent<SpriteRenderer>();
        _shaderGUItext = Shader.Find("GUI/Text Shader");
        Whiteify();
    }

    public void Whiteify()
    {
        _renderer.material.shader = _shaderGUItext;
        _renderer.color = Color.white;
    }
}
