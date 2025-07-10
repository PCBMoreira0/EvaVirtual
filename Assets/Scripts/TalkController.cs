using System.Collections;
using TMPro;
using UnityEngine;

public class TalkController : MonoBehaviour
{
    [SerializeField] private ChangeDialogue dialogueBox;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private APIComunication api;
    [SerializeField] private float noTTSTalkingDuration = 2f;

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
        HideDialog();
    }

    private void ShowDialog(string text)
    {
        dialogueBox.SetText(text);
        dialogueBox.gameObject.SetActive(true);
    }

    private void HideDialog()
    {
        dialogueBox.gameObject.SetActive(false);
    }

    public IEnumerator PlayTTS(string text)
    {
        AudioClip audioClip = null;
        yield return StartCoroutine(api.GetTTS(text, (result) => { audioClip = result; }));
        if (audioClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
            ShowDialog(text);
            yield return new WaitForSeconds(audioClip.length);
            HideDialog();
        }
        else
        {
            ShowDialog(text);
            yield return new WaitForSeconds(noTTSTalkingDuration);
            HideDialog();
        }

    }
    public IEnumerator Talk(string text)
    {
        if (tts_enabled)
        {
            yield return StartCoroutine(PlayTTS(text));
        }
        else
        {
            ShowDialog(text);
            yield return new WaitForSeconds(noTTSTalkingDuration);
        }
    }
}
