using System;
using System.Collections;
using UnityEngine;

public enum MotionTypes
{
    YES,
    NO,
    TWO_UP,
    TWO_DOWN,
    LEFT,
    RIGHT
}

public class MotionController : MonoBehaviour
{
    public float turnSpeed = 20f;
    [Header("Body Parts")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform head;

    [Header("Configs")]
    [SerializeField] MotionConfig motionConfigurations;

    private Coroutine currentCoroutine;

    [Serializable]
    private class MotionConfig
    {
        [Header("\"2UP-DOWN\" Motion")]
        public float twoUpDown_degree = 20f;

        [Header("\"LEFT-RIGHT\" Motion")]
        public float leftRight_degree = 20f; 
    }

    public void DebugMotion()
    {
        //StartCoroutine(Motion("head", MotionTypes.NO));
        StartCoroutine(ResetPosition());
    }

    public IEnumerator Motion(string member, string type)
    {
        switch (type)
        {
            case "YES":
                yield return Motion(member, MotionTypes.YES); 
                break;
            case "NO":
                yield return Motion(member, MotionTypes.NO);
                break;
            case "LEFT":
                yield return Motion(member, MotionTypes.LEFT);
                break;
            case "RIGHT":
                yield return Motion(member, MotionTypes.RIGHT);
                break;
            case "2UP":
                yield return Motion(member, MotionTypes.TWO_UP);
                break;
            case "2DOWN":
                yield return Motion(member, MotionTypes.TWO_DOWN);
                break;
        }
    }

    public IEnumerator Motion(string member, MotionTypes type)
    {
        if (currentCoroutine != null) yield break;

        if (member.CompareTo("head") == 0)
        {
            switch (type)
            {
                case MotionTypes.YES:
                    currentCoroutine = StartCoroutine(MoveHeadYES());
                    break;
                case MotionTypes.NO:
                    currentCoroutine = StartCoroutine(MoveHeadNO());
                    break;
                case MotionTypes.TWO_UP:
                    currentCoroutine = StartCoroutine(MoveHead2Up());
                    break;
                case MotionTypes.TWO_DOWN:
                    currentCoroutine = StartCoroutine(MoveHead2Down());
                    break;
                case MotionTypes.LEFT:
                    currentCoroutine = StartCoroutine(MoveHeadLeft());
                    break;
            }

            yield return currentCoroutine;
            StopMotion();
        }
    }

    private void StopMotion()
    {
        if (currentCoroutine == null) return;
        StopCoroutine(currentCoroutine);
        currentCoroutine = null;
    }

    private IEnumerator MoveHeadYES()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return StartCoroutine(MoveHead2Up());
            yield return StartCoroutine(MoveHead2Down());
        }
    }

    private IEnumerator MoveHeadNO()
    {
        yield return StartCoroutine(MoveHeadRight());
        yield return StartCoroutine(MoveHeadLeft());
        yield return StartCoroutine(MoveHeadLeft());
        yield return StartCoroutine(MoveHeadRight());
    }

    private IEnumerator MoveHead2Up()
    {
        float currentTime = 0f;
        float time = motionConfigurations.twoUpDown_degree / turnSpeed;
        while (currentTime < time)
        {
            head.Rotate(motionConfigurations.twoUpDown_degree * Time.deltaTime, 0, 0);

            currentTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator MoveHead2Down()
    {
        float currentTime = 0f;
        float time = motionConfigurations.twoUpDown_degree / turnSpeed;
        while (currentTime < time)
        {
            head.Rotate(-motionConfigurations.twoUpDown_degree * Time.deltaTime, 0, 0);

            currentTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator MoveHeadLeft()
    {
        float currentTime = 0f;
        float time = motionConfigurations.leftRight_degree / turnSpeed;
        while (currentTime < time)
        {
            head.Rotate(0, -motionConfigurations.leftRight_degree * Time.deltaTime, 0);

            currentTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator MoveHeadRight()
    {
        float currentTime = 0f;
        float time = motionConfigurations.leftRight_degree / turnSpeed;
        while (currentTime < time)
        {
            head.Rotate(0, motionConfigurations.leftRight_degree * Time.deltaTime, 0);

            currentTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator ResetPosition()
    {
        while (Quaternion.Angle(head.localRotation, Quaternion.identity) > 0.01f)
        {
            var a = Quaternion.RotateTowards(head.localRotation, Quaternion.identity, turnSpeed * Time.deltaTime);
            head.localRotation = a;
            yield return new WaitForEndOfFrame();
        }
        head.localRotation = Quaternion.identity;
    }
}
