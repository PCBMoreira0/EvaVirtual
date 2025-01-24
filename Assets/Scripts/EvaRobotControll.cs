using JetBrains.Annotations;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EvaRobotControll : MonoBehaviour
{
    [SerializeField] private APIComunication webCommunication;

    #region Events
    public UnityEvent<string> commandTalk;
    public UnityEvent<EmotionType> commandEmotion;
    #endregion


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
            Parser(currentCommand);
            yield return new WaitForSeconds(2);
           
        } while (currentCommand != null && currentCommand.command != "End of script");
    }

    public void Parser(APIComunication.CommandJson command)
    {
        if (command == null) return;

        switch (command)
        {
            case APIComunication.CommandTalkJson:
                commandTalk?.Invoke(((APIComunication.CommandTalkJson)command).text);
                break;
            case APIComunication.CommandEmotionJson:
                commandEmotion?.Invoke(Enum.Parse<EmotionType>(((APIComunication.CommandEmotionJson)command).emotion));
                break;
        }
    }
}
