using System.Collections;
using UnityEngine;

public class ExitController : MonoBehaviour
{
    GameManager gameManager;
    SFXManager sfxManager;
    SoundtrackManager soundtrackManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        sfxManager = FindFirstObjectByType<SFXManager>();
        soundtrackManager = FindAnyObjectByType<SoundtrackManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && gameManager.EnemiesRemaining == 0)
        {
            StartCoroutine(TeleportPlayerOne(collision.gameObject));
        }
    }

    private IEnumerator TeleportPlayerOne(GameObject playerOne)
    {
        Vector3 offset = new(0, 0.3f, 0);

        soundtrackManager.gameObject.SetActive(false);
        sfxManager.PlayClip(sfxManager.stageClear);

        playerOne.GetComponent<PlayerOneController>().InputActions.Disable();
        playerOne.transform.position = transform.position + offset;
        playerOne.GetComponent<Animator>().SetTrigger("teleport");

        yield return new WaitForSeconds(3.5f);

        playerOne.SetActive(false);
        gameManager.GoToNextLevel();
    }

}
