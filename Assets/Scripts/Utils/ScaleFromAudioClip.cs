using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScaleFromAudioClip : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] private TMP_Dropdown drop;
    [SerializeField] private TMP_Text text;
    private AudioClip micClip;
    private string selectedDevice;
    [SerializeField] private float sensivity = 1f;

    private void OnEnable()
    {
   
        selectedDevice = drop.options[0].text;
        Debug.Log(selectedDevice);
        micClip = Microphone.Start(selectedDevice, true, 20, 16000);
    }

    private void OnDisable()
    {
        if (Microphone.IsRecording(selectedDevice))
        {
            Microphone.End(selectedDevice);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float loudness = AudioOperations.GetLoudnessAverage(micClip, Microphone.GetPosition(selectedDevice), 64);
        text.text = loudness.ToString();
        slider.value = loudness * sensivity;
    }
}
