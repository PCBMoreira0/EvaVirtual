using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] List<MonoBehaviour> lightChanger;
    private List<ILightChanger> lightChanger_interface = new List<ILightChanger>();

    private void Awake()
    {
        foreach (var lightc in lightChanger)
        {
            if (lightc.TryGetComponent<ILightChanger>(out ILightChanger light))
            {
                lightChanger_interface.Add(light);
            }
            else
            {
                Debug.LogError($"O componente {lightc.name} atribuído não implementa ILightChanger.");
            }
        }
    }

    public IEnumerator ChangeLight(string color, string state)
    {
        if (gameObject.activeSelf == false) yield break;
        if (state == "OFF")
        {
            SetColor(Color.white);
            yield break;
        }

        SetColor(GetColor(color));
    }

    private void SetColor(Color color)
    {
        foreach (var light in lightChanger_interface)
        {
            light.ChangeColor(color);
        }
    }

    private Color GetColor(string color)
    {
        switch (color)
        {
            case "WHITE":
                return Color.white;
            case "BLACK":
                return Color.black;
            case "RED":
                return Color.red;
            case "PINK":
                return new Color(1.0f, 0.753f, 0.796f);
            case "GREEN":
                return Color.green;
            case "YELLOW":
                return Color.yellow;
            case "BLUE":
                return Color.blue;
            default:
                return Color.white;
        }
    }

    private void OnValidate()
    {
        if (lightChanger.Count == 0) return;
        foreach (var light in lightChanger)
        {
            if (light != null && !(light.TryGetComponent<ILightChanger>(out ILightChanger l)))
            {
                Debug.LogWarning($"{light.name} não implementa IDialogo!");
                lightChanger.Remove(light);
            }
        }
    }
}
