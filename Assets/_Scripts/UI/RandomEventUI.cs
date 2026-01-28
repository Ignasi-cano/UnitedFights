using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RandomEventUI : PersistentSingleton<RandomEventUI>
{
    [Header("UI References")]
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private RewardButton choicePrefab;

    public void Open(MapEventData eventData)
    {
        Debug.Log($"[RandomEventUI] Open called for event: {(eventData != null ? eventData.EventTitle : "NULL")}");
        if (eventData == null) 
        {
            Debug.LogWarning("[RandomEventUI] Open called with null eventData!");
            return;
        }

        if (eventPanel == null)
        {
            Debug.LogError($"[RandomEventUI] ERROR: 'Event Panel' is not assigned in the Inspector on {gameObject.name}!");
            return;
        }

        if (titleText == null || descriptionText == null || choicesContainer == null || choicePrefab == null)
        {
            Debug.LogError($"[RandomEventUI] ERROR: One or more UI references (Title, Description, Container, Prefab) are missing on {gameObject.name}!");
            return;
        }

        gameObject.SetActive(true); // Ensure the script's own object is on
        eventPanel.SetActive(true);
        
        if (titleText != null) {
             titleText.gameObject.SetActive(true);
             titleText.text = eventData.EventTitle;
        }
        
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = eventData.EventDescription;
        }

        if (illustrationImage != null) {
            illustrationImage.gameObject.SetActive(true);
            illustrationImage.sprite = eventData.Illustration;
        }

        if (choicesContainer != null) {
            choicesContainer.gameObject.SetActive(true);
        }

        // Clear previous choices
        foreach (Transform child in choicesContainer) Destroy(child.gameObject);

        // Spawn new choices
        foreach (var choice in eventData.Choices)
        {
            RewardButton btn = Instantiate(choicePrefab, choicesContainer);
            
            // Build outcome preview text
            string outcomePreview = "";
            foreach (var outcome in choice.Outcomes)
            {
                outcomePreview += (string.IsNullOrEmpty(outcomePreview) ? "" : "\n") + outcome.GetResultText();
            }

            btn.Setup(choice.ChoiceIcon, choice.ChoiceLabel, outcomePreview, () => {
                ExecuteChoice(choice);
            });
        }
    }

    private void ExecuteChoice(MapEventChoice choice)
    {
        Debug.Log($"[RandomEventUI] Choice selected: <b>{choice.ChoiceLabel}</b>. Executing {choice.Outcomes.Count} outcomes...");
        
        try 
        {
            // Execute all outcomes
            foreach (var outcome in choice.Outcomes)
            {
                if (outcome == null) continue;
                Debug.Log($"[RandomEventUI] Executing outcome: {outcome.GetResultText()}");
                outcome.Execute();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RandomEventUI] CRITICAL: Error during outcome execution! Choice: {choice.ChoiceLabel}. Error: {e.Message}\n{e.StackTrace}");
        }

        Debug.Log($"[RandomEventUI] Choice execution finished. Closing UI.");
        Close();
    }

    public void Close()
    {
        Debug.Log("[RandomEventUI] Close called. Hiding UI elements...");
        
        // Deactivate all specifically referenced objects in case they aren't children of eventPanel
        if (eventPanel != null) eventPanel.SetActive(false);
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (descriptionText != null) descriptionText.gameObject.SetActive(false);
        if (illustrationImage != null) illustrationImage.gameObject.SetActive(false);
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

        gameObject.SetActive(false); // Force hide the whole manager
        
        if (MapSystem.HasInstance)
        {
            MapSystem.Instance.RefreshMap();
        }
    }
}
