using UnityEngine;
using UnityEngine.SceneManagement;

public class ArrowController : MonoBehaviour
{
    SFXManager sfxManager;
    SpriteRenderer spriteRenderer;
    float enableAt;
    int currentIndex = 0;

    public Transform[] arrowPositions;
    
    void Start()
    {
        sfxManager = FindFirstObjectByType<SFXManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;

        enableAt = Time.time + 9.27f;
        transform.position = arrowPositions[currentIndex].position;
    }

    void Update()
    {
        EnableSelection();
    }

    private void EnableSelection()
    {
        if (Time.time >= enableAt)
        {
            if (!spriteRenderer.enabled) spriteRenderer.enabled = true;
            SwitchOption();
        }
    }

    private void SwitchOption()
    {
        if (currentIndex == 0 && Input.GetKeyDown(KeyCode.Z))
        {
            sfxManager.PlayClip(sfxManager.titleScreenSelect);
            Invoke("LoadStageScene", sfxManager.titleScreenSelect.length);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.W))
        {
            sfxManager.PlayClip(sfxManager.titleScreenCursor);

            if (Input.GetKeyDown(KeyCode.S)) currentIndex = (currentIndex + 1) % arrowPositions.Length;
            else if (Input.GetKeyDown(KeyCode.W)) currentIndex = (currentIndex - 1 + arrowPositions.Length) % arrowPositions.Length;

            transform.position = arrowPositions[currentIndex].position;
        }
    }

    private void LoadStageScene()
    {
        SceneManager.LoadScene("Stage 1-1");
    }

}
