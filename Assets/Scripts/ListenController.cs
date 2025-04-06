using System;
using System.Collections;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class ListenController : MonoBehaviour
{
    public APIComunication api;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private TMP_Dropdown drop;
    [SerializeField] private GameObject listeningActive;

    [SerializeField] private float silent_threshold; 

    private string selectedDevice = "";
    public RunWhisper runWhisper;

    private void Start()
    {
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

    public void ChangeDeviceFromDropDown(int value)
    {
        selectedDevice = drop.options[value].text;
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

    public IEnumerator StartRecording(Action<string> result)
    {
        listeningActive.SetActive(true);
        AudioClip audioClip = Microphone.Start(selectedDevice, false, 30, 16000);

        int lastMicPos = 0; 
        while (Microphone.IsRecording(selectedDevice))
        {
            yield return new WaitForSeconds(3);
            float loudness = GetLoundnessAverage(audioClip, Microphone.GetPosition(selectedDevice));
            Debug.Log(loudness);
            if(loudness <= silent_threshold)
            {
                lastMicPos = Microphone.GetPosition(selectedDevice);
                StopRecording();
            }
        }
        listeningActive.SetActive(false);

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