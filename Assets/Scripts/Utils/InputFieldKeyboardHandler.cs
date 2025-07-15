using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldKeyboardHandler : MonoBehaviour
{
    [SerializeField] TMP_InputField input;

    private Transform originalTransform;

    [SerializeField] private float heightValue;

    private void Start()
    {
        originalTransform = input.transform;
    }

    public void AdjustHeight()
    {

        input.transform.position = Vector3.up * heightValue;
    }

    public void ResetPosition()
    {
        input.transform.position = originalTransform.position;
    }
}
