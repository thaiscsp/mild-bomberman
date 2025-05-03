using System.Collections;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip boss, levelStart, stageOneIntro, stageOneLoop;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        PlayLevelBGM();
    }

    public IEnumerator PlayClip(AudioClip audioClip, float delay = 0, bool loop = false)
    {
        yield return new WaitForSeconds(delay);

        audioSource.loop = loop;
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    private void PlayLevelBGM()
    {
        StartCoroutine(PlayClip(levelStart));

        if (DataManager.instance.level < 8)
        {
            StartCoroutine(PlayClip(stageOneIntro, levelStart.length));
            StartCoroutine(PlayClip(stageOneLoop, levelStart.length + stageOneIntro.length, true));
        }
        else
        {
            StartCoroutine(PlayClip(boss, levelStart.length + 1, true));
        }
    }

}
