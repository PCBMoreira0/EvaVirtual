using JetBrains.Annotations;
using System.Collections;
using TMPro;
using UnityEngine;

public class EvaRobotControll : MonoBehaviour
{
    private APIComunication.CommandJson currentCommmand;
    public APIComunication webCommunication;

    public TextMeshProUGUI dialogueBox;

    public void StartRobot()
    {
        StartCoroutine(Execute());
    }
    
    IEnumerator Execute()
    {
        do
        {
            yield return StartCoroutine(webCommunication.NextCommand());
            Parser(currentCommmand);
            yield return new WaitForSeconds(2);
        } while (currentCommmand.command != "End of script");
    }

    public void ParseCommand(APIComunication.CommandJson command)
    {
        currentCommmand = command;
    }

    public void Parser(APIComunication.CommandJson command)
    {
        switch (command.command)
        {
            case "Talk":
                if (command is APIComunication.CommandTalkJson cTalk)
                {
                    Debug.Log(cTalk.talk);
                    TalkCommand(cTalk.talk);
                }
                break;
        }
    }

    public void TalkCommand(string text)
    {
        dialogueBox.text = text;
        Debug.Log(text);
    }
}
