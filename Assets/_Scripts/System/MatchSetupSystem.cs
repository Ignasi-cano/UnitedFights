using UnityEngine;
using System.Collections.Generic;
using System;

public class MatchSeetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData; // Re-added for fallback/testing
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;
    private void Start()
    {
        Debug.Log("Enviorement loaded");
        
        // Use selected hero from GameManager if available, otherwise fall back to inspector reference (or null)
        HeroData selectedHero = (GameManager.Instance != null && GameManager.Instance.SelectedHero != null) 
            ? GameManager.Instance.SelectedHero 
            : heroData;

        if (selectedHero == null)
        {
             Debug.LogError("No HeroData found! Ensure GameManager has a selected hero or Assign one in Inspector.");
             return;
        }

        HeroSystem.Instance.Setup(selectedHero);
        EnemySystem.Instance.Setup(enemyDatas);
        CardSystem.Instance.Setup(selectedHero.Deck);
        CardSystem.Instance.AddRandomCardToDeck();
        PerkSystem.Instance.AddPerk(new Perk(perkData));
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);

    }
}
