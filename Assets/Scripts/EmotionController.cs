using NUnit.Framework;
using System;
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

    [SerializeField] private float transitionTime = 0.5f;
    private Texture[] emotions = new Texture[8];
    private EmotionType currentEmotion = EmotionType.NEUTRAL;

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
        currentEmotion = newEmotion;  
        screen.materials[1].SetTexture("_BaseMap", emotions[(int)newEmotion]);
    }

    public IEnumerator ChangeEmotion_routine(EmotionType newEmotion)
    {
        int current_index = (int)currentEmotion;
        int dir = (int)Mathf.Sign((int)newEmotion - (int)currentEmotion);
        for (int i = (int)currentEmotion; i != (int)newEmotion; i += dir)
        {
            ChangeEmotion((EmotionType) i);
            yield return new WaitForSeconds(transitionTime);
        }

        ChangeEmotion(newEmotion);
    }
}
