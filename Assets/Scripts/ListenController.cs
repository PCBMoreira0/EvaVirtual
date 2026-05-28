using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListenController : MonoBehaviour
{
    [SerializeField] private APIComunication api;
    [SerializeField] private LedController ledController;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private TMP_Dropdown drop;
    [SerializeField] private TMP_InputField listenInputField;
    private bool keyboardToggle = true;

    [SerializeField] private float silent_threshold = 0.05f;
    [SerializeField] private float windowSize_seconds = 3;
    [SerializeField] private int subWindowSize = 8000;

    private string selectedDevice = "";

    private void Start()
    {
        //    loudneessField.text = silent_threshold.ToString();
        //    loudneessField.onEndEdit.AddListener((string inputText) => { silent_threshold = float.Parse(inputText); Debug.Log("New silent: " + silent_threshold); });

        if (Microphone.devices.Length == 0)
        {
            return;
        }

        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            drop.options.Add(new TMP_Dropdown.OptionData(Microphone.devices[i]));
        }

        drop.RefreshShownValue();
        selectedDevice = drop.options[0].text;
    }

    public void EnableKeyboard(bool value)
    {
        keyboardToggle = value;
    }

    public void UpdateLoudnessThreshold(float loudness)
    {
        silent_threshold = loudness;
    }

    public void ChangeDeviceFromDropDown(int value)
    {
        selectedDevice = drop.options[value].text;
    }

    public IEnumerator StartListening(Action<string> result)
    {
        if (keyboardToggle)
        {
            yield return GetKeyboardInput(result);
        }
        else
        {
            yield return StartRecording(result);
        }
    }

    private IEnumerator GetKeyboardInput(Action<string> result)
    {
        listenInputField.gameObject.SetActive(true);

        string input = "";
        bool eventHappen = false;
        listenInputField.text = "";
        UnityAction<string> action = (string inputText) => { input = inputText; eventHappen = true; };
        listenInputField.onEndEdit.AddListener(action);
        yield return new WaitUntil(() => eventHappen);

        listenInputField.onEndEdit.RemoveListener(action);

        result?.Invoke(input);
        listenInputField.gameObject.SetActive(false);

        ledController.ResetColor();
    }

    public void ResetListen()
    {
        Microphone.End(selectedDevice);
    }

    public void TurnMicrophone()
    {
        if (Microphone.IsRecording(selectedDevice))
        {

            StopRecording();
        }
        else
        {

            StartCoroutine(StartRecording(null));
        }
    }

    public void StartMiccc()
    {
        string result;
        //StartCoroutine(StartRecording((res) => { result = res; }));
    }

    private IEnumerator StartRecording(Action<string> result)
    {
        AudioClip audioClip = Microphone.Start(selectedDevice, true, 30, 16000);

        int lastMicPos = 0;
        bool alreadyPassedTheshold = false;

        int sampleWindow = (int)(windowSize_seconds * 16000);

        while (Microphone.GetPosition(selectedDevice) < sampleWindow)
        {
            yield return null;
        }

        while (Microphone.IsRecording(selectedDevice))
        {
            //float loudness = AudioOperations.GetLoudnessAverage(audioClip, Microphone.GetPosition(selectedDevice), sampleWindow);
            bool loudness = AudioOperations.IsAnySubwindowLoud(audioClip, Microphone.GetPosition(selectedDevice), sampleWindow, subWindowSize, silent_threshold);
            Debug.Log("Thesh: " + silent_threshold + " Loud: " + loudness);

            if (!loudness) //loudness <= silent_threshold
            {
                if (alreadyPassedTheshold)
                {
                    lastMicPos = Microphone.GetPosition(selectedDevice);
                    StopRecording();
                    break;
                }
            }
            else
            {
                alreadyPassedTheshold = true;
            }

            yield return new WaitForSeconds(windowSize_seconds);
        }

        while (Microphone.IsRecording(selectedDevice)) yield return null;

        ledController.ResetColor();
        float[] data = new float[lastMicPos];
        audioClip.GetData(data, 0);
        AudioClip trimm = AudioClip.Create("trimmedAudio", lastMicPos, audioClip.channels, audioClip.frequency, false);

        trimm.SetData(data, 0);
        yield return api.GetSTT(trimm, result);
        StopAllCoroutines();
    }


    private void StopRecording()
    {
        Microphone.End(selectedDevice);
    }
}