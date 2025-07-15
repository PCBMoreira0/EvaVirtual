using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListenController : MonoBehaviour
{
    public APIComunication api;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private TMP_Dropdown drop;
    [SerializeField] private TMP_InputField listenInputField;
    [SerializeField] private Toggle keyboardToggle;
    [SerializeField] private TMP_InputField loudneessField;

    [SerializeField] private float silent_threshold; 

    private string selectedDevice = "";

    private void Start()
    {
        loudneessField.text = silent_threshold.ToString();
        loudneessField.onEndEdit.AddListener((string inputText) => { silent_threshold = float.Parse(inputText); Debug.Log("New silent: " + silent_threshold); });

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

    public void UpdateLoudnessThreshold(string loudness)
    {
        if(float.TryParse(loudness, out float result))
        {
            silent_threshold = result;
        }
    }

    public void ChangeDeviceFromDropDown(int value)
    {
        selectedDevice = drop.options[value].text;
    }

    public IEnumerator StartListening(Action<string> result, LedController led)
    {
        
        if(keyboardToggle.isOn)
        {
            yield return StartCoroutine(GetKeyboardInput(result));
        }
        else
        {
            yield return StartCoroutine(StartRecording(result, led));
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
    }

    public void TurnMicrophone()
    {
        if (Microphone.IsRecording(selectedDevice))
        {

            StopRecording();
        }
        else
        {

            StartCoroutine(StartRecording(null, null));
        }
    }


    private float GetLoundnessAverage(AudioClip clip, int micPos)
    {
        const int sampleWindow = 3 * 16000;
        float[] data = new float[sampleWindow];
        clip.GetData(data, Mathf.Clamp(micPos - sampleWindow, 0, clip.samples-sampleWindow));


        // Pesquisar sobre fórmulas para calcular altura do áudio

        float sample_sum = 0;
        foreach (float audio_sample in data) 
        {
            sample_sum += Mathf.Abs(audio_sample);
        }

        return sample_sum / sampleWindow;
    }

    public void StartMiccc()
    {
        string result;
        //StartCoroutine(StartRecording((res) => { result = res; }));
    }

    private IEnumerator StartRecording(Action<string> result, LedController led)
    {
        AudioClip audioClip = Microphone.Start(selectedDevice, false, 30, 16000);

        int lastMicPos = 0;
        bool alreadyPassedTheshold = false;
        while (Microphone.IsRecording(selectedDevice))
        {
            yield return new WaitForSeconds(3);
            float loudness = GetLoundnessAverage(audioClip, Microphone.GetPosition(selectedDevice));
            Debug.Log(loudness);

            if(loudness <= silent_threshold)
            {
                if (alreadyPassedTheshold)
                {
                    lastMicPos = Microphone.GetPosition(selectedDevice);
                    StopRecording();
                }
            }
            else
            {
                alreadyPassedTheshold = true;
            }
        }

        led.ResetColor();

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