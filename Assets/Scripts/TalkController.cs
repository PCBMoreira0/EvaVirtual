using System.Collections;
using TMPro;
using UnityEngine;

public class TalkController : MonoBehaviour
{
    [SerializeField] private ChangeDialogue dialogueBox;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private APIComunication api;

    private bool tts_enabled = true;

    private void Awake()
    {
        api = GetComponent<APIComunication>();
    }

    public void EnableTTS()
    {
        if (tts_enabled)
            tts_enabled = false;
        else tts_enabled = true;
    }

    public void ResetDialogueBox()
    {
        dialogueBox.SetText("");
    }

    public IEnumerator PlayTTS(string text)
    {
        AudioClip audioClip = null;
        yield return StartCoroutine(api.GetTTS(text, (result) => { audioClip = result; }));
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }

    }
    public IEnumerator Talk(string text)
    {
        if(tts_enabled)
            yield return StartCoroutine(PlayTTS(text));

        dialogueBox.SetText(text);
    }
}
