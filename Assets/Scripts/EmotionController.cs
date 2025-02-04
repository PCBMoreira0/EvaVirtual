using NUnit.Framework;
using System.Collections;
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

public class EmotionController : MonoBehaviour
{
    [SerializeField] private Renderer screen;

    [Header("Emotions")]
    [SerializeField] Texture angry;
    [SerializeField] Texture disgust;
    [SerializeField] Texture fear;
    [SerializeField] Texture happy;
    [SerializeField] Texture inlove;
    [SerializeField] Texture neutral;
    [SerializeField] Texture sad;
    [SerializeField] Texture surprise;

    private Texture[] emotions = new Texture[8];

    private void Awake()
    {
        emotions[0] = angry;
        emotions[1] = disgust;
        emotions[2] = fear;
        emotions[3] = happy;
        emotions[4] = inlove;
        emotions[5] = neutral;
        emotions[6] = sad;
        emotions[7] = surprise;
    }
    public void ChangeEmotion(EmotionType newEmotion)
    {
        screen.materials[1].SetTexture("_BaseMap", emotions[(int)newEmotion]);
    }
}
