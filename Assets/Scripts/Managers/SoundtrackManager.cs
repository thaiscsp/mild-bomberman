using System.Collections;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip levelStart, stageOneIntro, stageOneLoop;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayClip(levelStart));
        StartCoroutine(PlayClip(stageOneIntro, levelStart.length));
        StartCoroutine(PlayClip(stageOneLoop, levelStart.length + stageOneIntro.length, true));
    }

    public IEnumerator PlayClip(AudioClip audioClip, float delay = 0, bool loop = false)
    {
        yield return new WaitForSeconds(delay);

        audioSource.loop = loop;
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
