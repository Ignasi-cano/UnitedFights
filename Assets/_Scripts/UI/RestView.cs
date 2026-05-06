using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RestView : MonoBehaviour
{
    public static RestView Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindObjectOfType<RestView>(true);
            }
            return _instance;
        }
    }
    private static RestView _instance;

    [SerializeField] private Button restButton;
    [SerializeField] private Button formationButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject restEffectVFX; // Optional visual fluff

    private bool hasRested = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        gameObject.SetActive(false); // Hide by default

        if (restButton != null)
            restButton.onClick.AddListener(OnRestClicked);

        if (formationButton != null)
            formationButton.onClick.AddListener(OnFormationClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseRest);
    }

    private void OnFormationClicked()
    {
        if (FormationView.Instance != null)
        {
            FormationView.Instance.OpenFormation();
        }
        else
        {
            Debug.LogError("[RestView] FormationView Instance not found in scene.");
        }
    }

    public void OpenRest()
    {
        gameObject.SetActive(true);
        hasRested = false;
        if (restButton != null) restButton.interactable = true;
    }

    private void OnRestClicked()
    {
        if (hasRested) return;

        HealAllHeroesToMax();
        hasRested = true;
        
        if (restButton != null) restButton.interactable = false;
        
        Debug.Log("[RestView] All heroes healed to maximum health.");
        
        if (restEffectVFX != null)
        {
            Instantiate(restEffectVFX, Vector3.zero, Quaternion.identity);
        }
    }

    private void HealAllHeroesToMax()
    {
        if (GameManager.Instance == null) return;

        foreach (var hero in GameManager.Instance.ActiveHeroes)
        {
            int maxHP = hero.GetMaxHealth();
            hero.CurrentHealth = maxHP;
            Debug.Log($"[RestView] Healed {hero.Data.name} to {maxHP}");
        }
    }

    public void CloseRest()
    {
        gameObject.SetActive(false);
        if (MapSystem.HasInstance)
        {
            MapSystem.Instance.RefreshMap();
        }
    }
}
