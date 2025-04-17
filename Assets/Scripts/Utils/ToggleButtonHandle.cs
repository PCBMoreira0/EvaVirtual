using System.Collections.Generic;
using UnityEngine;

public class ToggleButtonHandle : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToToggle;
    private bool isActive = false;

    public void Toggle()
    {
        foreach (GameObject obj in objectsToToggle)
        {
            if(isActive)
            {
                obj.SetActive(false);
                isActive = false;
            }
            else
            {
                obj.SetActive(true);
                isActive = true;
            }
        }
    }
}
