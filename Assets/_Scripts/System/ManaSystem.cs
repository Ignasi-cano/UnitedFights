using System;
using System.Collections;
using UnityEngine;

public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] private ManaUI manaUI;
    private const int MAX_MANA = 3;
    private int currentMana = MAX_MANA; 
   void OnEnable()
{
    ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
    ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);
    ActionSystem.AttachPerformer<AddManaGA>(AddManaPerformer);

    ActionSystem.SubscribeReaction<HeroTurnStartGA>(
        OnHeroTurnStartReaction,
        ReactionTiming.POST
    );
}

void OnDisable()
{
    ActionSystem.DetachPerformer<SpendManaGA>();
    ActionSystem.DetachPerformer<RefillManaGA>();
    ActionSystem.DetachPerformer<AddManaGA>();

    ActionSystem.UnsubscribeReaction<HeroTurnStartGA>(
        OnHeroTurnStartReaction,
        ReactionTiming.POST
    );
}

    public bool HasEnoughMana(int mana)
    {
        return currentMana >=mana;
    }
    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        currentMana -= spendManaGA.Amount;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }
    private IEnumerator AddManaPerformer(AddManaGA addManaGA)
    {
    currentMana += addManaGA.Amount;

    if (currentMana > MAX_MANA)
        currentMana = MAX_MANA;

    manaUI.UpdateManaText(currentMana);
    yield return null;
    }

    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        currentMana = MAX_MANA;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }
    private void OnHeroTurnStartReaction(HeroTurnStartGA action)
    {
        RefillManaGA refillManaGA = new();
        ActionSystem.Instance.AddReaction(refillManaGA);
    }
}
