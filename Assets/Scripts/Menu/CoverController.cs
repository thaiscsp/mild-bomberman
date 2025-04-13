using UnityEngine;

public class CoverController : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    TitleController titleController;
    public enum TransitionState { BlackFadeOut, WhiteFlashBegin, WhiteFlashEnd, None }
    public TransitionState transitionState { get; private set; }
    public bool titleMatched { get; set; }
    public float introSpeed, flashSpeed;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        titleController = FindFirstObjectByType<TitleController>();

        spriteRenderer.color = Color.black;
    }

    void Update()
    {
        SetTransitionState();
        ChangeColor();
    }

    private void SetTransitionState()
    {
        if (spriteRenderer.color == Color.black) transitionState = TransitionState.BlackFadeOut;
        else if (titleController.transform.position.x == titleController.xTarget && transitionState == TransitionState.BlackFadeOut) transitionState = TransitionState.WhiteFlashBegin;
        else if (spriteRenderer.color == Color.white) transitionState = TransitionState.WhiteFlashEnd;
        else if (spriteRenderer.color == Color.clear && transitionState == TransitionState.WhiteFlashEnd) transitionState = TransitionState.None;
    }

    private void ChangeColor()
    {
        switch (transitionState)
        {
            case TransitionState.BlackFadeOut:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, introSpeed * Time.deltaTime); // First transition from black screen to menu
                break;

            case TransitionState.WhiteFlashBegin:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, flashSpeed * Time.deltaTime); // White flash on title match
                break;

            case TransitionState.WhiteFlashEnd:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, flashSpeed * Time.deltaTime); // Show menu again after flash
                break;
        }
    }

}
