using System.Collections;
using UnityEngine;

public class LedController : MonoBehaviour
{
    [SerializeField] private Renderer led;
    [SerializeField] private float ledIntensity = 1f;

    public void ResetColor()
    {
        StartCoroutine(ChangeLedColor("STOP"));
    }

    public IEnumerator ChangeLedColor(string color)
    {
        led.material.color = GetColor(color);
        led.material.EnableKeyword("_EMISSION");

        if (GetColor(color) == Color.black)
        {
            led.material.SetColor("_BaseColor", Color.white);
        }
        else
        {
            led.material.SetColor("_BaseColor", GetColor(color));
        }

        led.material.SetColor("_EmissionColor", GetColor(color) * ledIntensity);

        yield return null;
    }

    private Color GetColor(string color)
    {
        switch (color)
        {
            case "LISTEN":
                return Color.green;
            case "grey":
                return Color.gray;
            case "SPEAK":
                return Color.blue;
            case "red":
                return Color.red;
            case "yellow":
                return Color.yellow;
            case "STOP":
                return Color.black;
            case "rainbow":
                return Color.magenta;
            default:
                return Color.black;
        }
    }
}
