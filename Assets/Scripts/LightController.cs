using System.Collections;
using UnityEngine;

public class LightController : MonoBehaviour
{
    Camera mainCamera;
    Color defaultColor = new Color(0.533f, 0.796f, 1.0f);

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public IEnumerator ChangeLight(string color, string state)
    {
        if (state == "OFF")
        {
            mainCamera.backgroundColor = defaultColor;
            yield break;
        }

        Color light = defaultColor;  
        switch (color)
        {
            case "WHITE":
                light = Color.white;
                break;
            case "BLACK":
                light = Color.black;
                break;
            case "RED":
                light = Color.red;
                break;
            case "PINK":
                light = new Color(1.0f, 0.753f, 0.796f);
                break;
            case "GREEN":
                light = Color.green;
                break;
            case "YELLOW":
                light = Color.yellow;
                break;
            case "BLUE":
                light = Color.blue;
                break;
        }

        mainCamera.backgroundColor = light;
    }
}
