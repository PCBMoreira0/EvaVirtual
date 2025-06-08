using System.Collections;
using UnityEngine;

public class LedController : MonoBehaviour
{
    [SerializeField] private Renderer led;

    public void ResetColor()
    {
        led.material.color = Color.white;
    }

    public IEnumerator ChangeLedColor(string color)
    {
        switch (color)
        {
            case "green":
                led.material.color = Color.green;
                break;
            case "grey":
                led.material.color = Color.gray;
                break;
            case "blue":
                led.material.color = Color.blue;
                break;
            case "red":
                led.material.color = Color.red;
                break;
            case "yellow":
                led.material.color = Color.yellow;
                break;
            case "white":
                led.material.color = Color.white;
                break;
            case "rainbow":
                led.material.color = Color.magenta;
                break;
        }

        yield return null;
    }
}
