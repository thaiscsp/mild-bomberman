using System.Collections;
using UnityEngine;

public class ExitController : MonoBehaviour
{
    private GameManager gameManager;
    private SFXManager sfxManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        sfxManager = FindFirstObjectByType<SFXManager>();
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

        sfxManager.PlayClip(sfxManager.stageClear);

        playerOne.GetComponent<PlayerOneController>().InputActions.Disable();
        playerOne.transform.position = transform.position + offset;
        playerOne.GetComponent<Animator>().SetTrigger("teleport");

        yield return new WaitForSeconds(3.5f);

        playerOne.SetActive(false);
    }

}
