using TMPro;
using UnityEngine;

public class ChangeTextSliderHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text textSlide;

   public void ChangeText(float value)
    {
        textSlide.text = (value).ToString();
    }
}
