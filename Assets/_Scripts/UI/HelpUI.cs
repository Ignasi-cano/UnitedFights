using UnityEngine;

public class HelpUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the 'HelpIMG' GameObject here from the Hierarchy.")]
    public GameObject helpPanel;

    // This public method will be called by the button's OnClick event
    public void OpenClick()
    {
        // Check if the variable has been assigned in the Inspector
        if (helpPanel != null)
        {
            helpPanel.SetActive(!helpPanel.activeSelf);
        }
        else
        {
            Debug.LogError("HelpIMG has not been assigned in the inspector!");
        }
    }
}