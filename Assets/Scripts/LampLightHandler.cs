using UnityEngine;

public class LampLightHandler : MonoBehaviour, ILightChanger
{
    [SerializeField] Light light;
    public void ChangeColor(Color color)
    {
        light.color = color;
    }
}
