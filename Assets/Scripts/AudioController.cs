using System.Collections;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private APIComunication api;

    public IEnumerator PlayAudio(string fileName, bool block)
    {
        AudioClip audioClip = null;
        yield return api.GetAudio(fileName, (AudioClip clip) => { audioClip = clip; });

        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);

            if (block)
            {
                yield return new WaitForSeconds(audioClip.length);
            }
        }
    }
}
