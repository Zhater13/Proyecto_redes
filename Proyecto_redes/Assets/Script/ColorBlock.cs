using UnityEngine;

public class ColorBlock : MonoBehaviour
    
{
    public GameColor BlockColor;

    SpriteRenderer spriteRenderer;
    Collider2D blockCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        blockCollider = GetComponent<Collider2D>();
    }

    public void Apply(GameColor background) // The block disappears and becomes intangible when the background matches its color
    {
        bool visible = background != BlockColor;
        spriteRenderer.enabled = visible;
        blockCollider.enabled = visible;
    }
}
