using UnityEngine;

public class SFXManager : MonoBehaviour
{
    private AudioSource audioSource;
    
    public AudioClip bombExplodes, bombermanDies, enemyDies,
        itemGet, placeBomb, stageClear,
        titleScreenCursor, titleScreenSelect, walkingOne;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}
