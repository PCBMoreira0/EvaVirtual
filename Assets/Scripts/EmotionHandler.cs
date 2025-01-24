using NUnit.Framework;
using UnityEngine;

public enum EmotionType
{
    ANGRY = 0,
    DISGUST = 1,
    FEAR = 2,
    HAPPY = 3,
    INLOVE = 4,
    NEUTRAL = 5,
    SAD = 6,
    SURPRISE = 7
}

public class EmotionHandler : MonoBehaviour
{
    public Renderer screen;

    [SerializeField] private Texture[] emotions = new Texture[8];

    public void ChangeEmotion(EmotionType newEmotion)
    {
        screen.materials[1].SetTexture("_BaseMap", emotions[(int)newEmotion]);
    }
}
