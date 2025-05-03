using UnityEngine;

public class TextController : MonoBehaviour
{
    CoverController coverController;
    SpriteRenderer spriteRenderer;

    public float transitionSpeed;

    void Start()
    {
        coverController = FindFirstObjectByType<CoverController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = Color.clear;   
    }

    void Update()
    {
        ChangeColor();   
    }

    private void ChangeColor()
    {
        if (coverController.transitionState == CoverController.TransitionState.None) spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, transitionSpeed * Time.deltaTime);
    }
}
