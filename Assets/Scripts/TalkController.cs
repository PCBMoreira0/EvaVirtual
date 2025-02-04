using TMPro;
using UnityEngine;

public class TalkController : MonoBehaviour
{
    [SerializeField] private ChangeDialogue dialogueBox;

    public void Talk(string text)
    {
        dialogueBox.SetText(text);
    }
}
