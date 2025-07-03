using TMPro;
using UnityEngine;

public class ChangeDialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;

    public void SetText(string text)
    {
        textMesh.SetText(text);
    }
}
