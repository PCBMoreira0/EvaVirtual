using System.Collections;
using UnityEngine;

public class LedController : MonoBehaviour
{
    [SerializeField] private Renderer led;

    public IEnumerator ChangeLedColor(string color)
    {
        switch (color)
        {
            case "green":
                led.material.SetColor("_Color", Color.green);
                break;
        }

        yield return null;
    }
}
