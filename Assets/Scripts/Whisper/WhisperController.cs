using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhisperController : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private TMP_Dropdown drop;
    [SerializeField] private Button button;

    private string selectedDevice = "";
    public RunWhisper runWhisper;

    private void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            textMeshProUGUI.SetText("No microphone available");
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
            
            StartRecording();
        }
    }

    private async void StartRecording()
    {
        audioClip = Microphone.Start(selectedDevice, true, 30, 16000);
        button.image.color = Color.red;
        
        while (Microphone.IsRecording(selectedDevice))
        {
            const int silentThreshold = 16000 * 3; // 3 seconds    
            var micPos = Microphone.GetPosition(selectedDevice);
            if (micPos > silentThreshold)
            {
                var data = new float[silentThreshold];
                audioClip.GetData(data, Microphone.GetPosition(selectedDevice) - silentThreshold);
                var silent = await runWhisper.Transcribe(data);
                if (silent.Contains("(") || silent.Contains("[") || silent.Contains(")") || silent.Contains("]"))
                {
                    StopRecording();
                }
            }

            var result = await runWhisper.Transcribe(audioClip);
            textMeshProUGUI.SetText(result);

            await Awaitable.NextFrameAsync();
        }

        button.image.color = Color.white;
    }

    private void StopRecording()
    {
        Microphone.End(selectedDevice);
    } 
}
