using UnityEngine;

public class Utilities : MonoBehaviour
{
    public GameObject WhiteSprite;

    public static Utilities Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SpritePop(SpriteRenderer original, Color color, bool flipped = false)
    {
        if (!original.isVisible) return;

        var obj = Instantiate(WhiteSprite) as GameObject;
        obj.transform.SetParent(original.transform, false);
        var renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sprite = original.sprite;
        renderer.color = new Color(color.r, color.g, color.b, renderer.color.a);

        //obj.transform.localScale = original.transform.localScale;
        obj.transform.localPosition = Vector3.back * .05f;
        if (flipped)
        {
            var scale = obj.transform.localScale;
            scale.x *= -1;
            obj.transform.localScale = scale;
        }
    }
}
