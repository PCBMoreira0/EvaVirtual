using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SaveMenuConfigs : MonoBehaviour
{
    [SerializeField] private TMP_InputField serverIP;
    [SerializeField] private Toggle bool_tts;
    [SerializeField] private Toggle bool_keyboard;
    [SerializeField] private TMP_InputField loudness;
    [SerializeField] private TMP_InputField evaML;

    private string serverIPKey = "server_ip";
    private string bool_ttsKey = "bool_tts";
    private string bool_keyboardKey = "bool_keyboard";
    private string loudnessKey = "loudness";
    private string evaMLKey = "evaML";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(bool_keyboard.isOn);
        if (PlayerPrefs.HasKey(serverIPKey))
        {
            serverIP.text = PlayerPrefs.GetString(serverIPKey);
            serverIP.onEndEdit?.Invoke(serverIP.text);
        }
        if(PlayerPrefs.HasKey(bool_ttsKey))
        {
            bool_tts.isOn = bool.Parse(PlayerPrefs.GetString(bool_ttsKey));
            bool_tts.onValueChanged?.Invoke(bool_tts.isOn);
        }
        if(PlayerPrefs.HasKey(bool_keyboardKey))
        {
            bool_keyboard.isOn = bool.Parse(PlayerPrefs.GetString(bool_keyboardKey));
            bool_keyboard.onValueChanged?.Invoke(bool_keyboard.isOn);
        }
        if (PlayerPrefs.HasKey(loudnessKey))
        {
            loudness.text = PlayerPrefs.GetString(loudnessKey);
            loudness.onEndEdit?.Invoke(loudness.text);
        }
        if(PlayerPrefs.HasKey(evaMLKey))
        {
            evaML.text = PlayerPrefs.GetString(evaMLKey);
            evaML.onEndEdit?.Invoke(evaML.text);
        }
    }

    public void SaveConfigs()
    {
        PlayerPrefs.SetString(serverIPKey, serverIP.text);
        PlayerPrefs.SetString(bool_ttsKey, bool_tts.isOn.ToString());
        PlayerPrefs.SetString(bool_keyboardKey, bool_keyboard.isOn.ToString());
        PlayerPrefs.SetString(loudnessKey, loudness.text);
        PlayerPrefs.SetString(evaMLKey, evaML.text);
        PlayerPrefs.Save();
    }
}
