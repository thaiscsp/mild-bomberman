using System.Collections;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip levelStart;
    public AudioClip worldOne;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayClip(levelStart));
        StartCoroutine(PlayClip(worldOne, levelStart.length, true));
    }

    public IEnumerator PlayClip(AudioClip audioClip, float delay = 0, bool loop = false)
    {
        yield return new WaitForSeconds(delay);

        audioSource.loop = loop;
        audioSource.PlayOneShot(audioClip);
    }
}
