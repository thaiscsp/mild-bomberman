using UnityEngine;

public class StoneController : MonoBehaviour
{
    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;
    float breakDelay;

    public Vector2 Force { get; set; }

    public Sprite smallStoneSprite;

    void Start()
    {
        breakDelay = Random.Range(0.7f, 0.9f);

        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        Push(Force);

        Invoke("Break", breakDelay);
    }

    void Update()
    {
        
    }

    private void Push(Vector2 force)
    {
        rigidBody.AddForce(force, ForceMode2D.Impulse);
    }

    private void Break()
    {
        spriteRenderer.sprite = smallStoneSprite;
        rigidBody.velocity = Vector3.zero;

        Push(Force/2);
        Invoke("DestroyStone", breakDelay * 0.75f);
    }

    private void DestroyStone()
    {
        Destroy(gameObject);
    }

}
