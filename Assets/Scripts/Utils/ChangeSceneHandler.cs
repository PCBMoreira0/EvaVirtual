using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneHandler : MonoBehaviour
{
    public void ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }
}
