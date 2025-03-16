using UnityEngine;

public class SFXManager : MonoBehaviour
{
    private AudioSource audioSource;
    
    public AudioClip bombExplodes;
    public AudioClip bombermanDies;
    public AudioClip enemyDies;
    public AudioClip itemGet;
    public AudioClip placeBomb;
    public AudioClip stageClear;
    public AudioClip walkingOne;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}
