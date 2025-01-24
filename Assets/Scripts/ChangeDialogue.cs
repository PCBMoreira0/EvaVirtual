using TMPro;
using UnityEngine;

public class ChangeDialogue : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    private void Awake()
    {
        this.textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(string text)
    {
        textMesh.SetText(text);
    }
}
