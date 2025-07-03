using UnityEngine;
using UnityEngine.UI;

public class ImageLightHandler : MonoBehaviour, ILightChanger
{
    [SerializeField] private Image image;
    public void ChangeColor(Color color)
    {
        image.color = color;
    }
}
