using UnityEngine;
using UnityEngine.SceneManagement;

public class CoverController : MonoBehaviour
{
    ArrowController arrowController;
    SpriteRenderer spriteRenderer;
    TitleController titleController;
    float fadeInAt;

    public enum TransitionState { BlackFadeOut, WhiteFlashBegin, WhiteFlashEnd, BlackFadeIn, None }
    public TransitionState transitionState { get; set; }
    public bool titleMatched { get; set; }
    public float introSpeed, flashSpeed, exitSpeed;

    void Start()
    {
        arrowController = FindFirstObjectByType<ArrowController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        titleController = FindFirstObjectByType<TitleController>();

        spriteRenderer.color = Color.black;
        fadeInAt = Time.time + 7.5f;
    }

    void Update()
    {
        SetTransitionState();
        ChangeColor();
        LoadStageScene();
    }

    private void SetTransitionState()
    {
        if (spriteRenderer.color == Color.black) transitionState = TransitionState.BlackFadeOut;

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            if (titleController.transform.position.x == titleController.xTarget && transitionState == TransitionState.BlackFadeOut) transitionState = TransitionState.WhiteFlashBegin;
            else if (spriteRenderer.color == Color.white) transitionState = TransitionState.WhiteFlashEnd;
            else if (spriteRenderer.color == Color.clear && transitionState == TransitionState.WhiteFlashEnd) transitionState = TransitionState.None;
            else if (arrowController.optionSelected) transitionState = TransitionState.BlackFadeIn;
        }
        else if (SceneManager.GetActiveScene().name == "World Map" && Time.time >= fadeInAt) transitionState = TransitionState.BlackFadeIn;
    }

    private void ChangeColor()
    {
        switch (transitionState)
        {
            case TransitionState.BlackFadeOut:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, introSpeed * Time.deltaTime); // First transition from black screen to menu/world map
                break;

            case TransitionState.WhiteFlashBegin:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, flashSpeed * Time.deltaTime); // White flash on title match
                break;

            case TransitionState.WhiteFlashEnd:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, flashSpeed * Time.deltaTime); // Show menu again after flash
                break;

            case TransitionState.BlackFadeIn:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.black, exitSpeed * Time.deltaTime); // Turn screen black again
                break;
        }
    }

    private void LoadStageScene()
    {
        if (spriteRenderer.color == Color.black && transitionState == TransitionState.BlackFadeIn && SceneManager.GetActiveScene().name == "World Map")
        {
            SceneManager.LoadScene("Stage 1-1");
        }
    }
}
