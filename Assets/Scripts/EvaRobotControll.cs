using JetBrains.Annotations;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
public class EvaRobotControll : MonoBehaviour
{
    [SerializeField] private APIComunication webCommunication;
    [SerializeField] private float timeBetweenCommands = 2f;

    #region Events
    public UnityEvent<APIComunication.CommandJson> OnCommandReceived;
    #endregion


    #region Controllers
    private TalkController talkController;
    private EmotionController emotionController;
    private MotionController motionController;
    #endregion

    private void Awake()
    {
        talkController = GetComponent<TalkController>();
        emotionController = GetComponent<EmotionController>();
        motionController = GetComponent<MotionController>();
    }

    private void Start()
    {
        webCommunication.OnInitializationComplete += StartRobot;
    }

    public void StartRobot()
    {
        StartCoroutine(Execute());
    }
    
    IEnumerator Execute()
    {
        APIComunication.CommandJson currentCommand = null;
        do
        {
            yield return StartCoroutine(webCommunication.NextCommand((result) => { currentCommand = result; }));
            yield return Parser(currentCommand);
            yield return new WaitForSeconds(timeBetweenCommands);
        } while (currentCommand != null && currentCommand.command != "End of script");

        // Reset position
        yield return motionController.ResetPosition();
    }

    private IEnumerator Parser(APIComunication.CommandJson command)
    {
        if (command == null) yield break;

        switch (command)
        {
            case APIComunication.CommandTalkJson commandTalkJson:
                if (talkController != null) 
                    talkController.Talk(commandTalkJson.text);
                break;

            case APIComunication.CommandEmotionJson emotionCommand:
                if (emotionController != null)
                    emotionController.ChangeEmotion(Enum.Parse<EmotionType>(emotionCommand.emotion));
                break;

            case APIComunication.CommandMotionJson motionCommand:
                if (motionController != null)
                    yield return StartCoroutine(motionController.Motion(motionCommand.member, motionCommand.direction));
                break;
        }

        OnCommandReceived?.Invoke(command);
        yield return null;
    }
}
