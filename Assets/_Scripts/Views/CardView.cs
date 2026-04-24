using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private TMP_Text cardTypeText;
    [SerializeField] private TMP_Text ownerText;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private SpriteRenderer tintableBackgroundSR;
    [SerializeField] private SpriteRenderer typeBannerSR;

    [Header("Interaction")]
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;

    public Card Card { get; private set; }

    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;

    public void Setup(Card card)
    {
        Card = card;

        title.text = card.Title;
        description.text = GetDynamicDescription(card);
        mana.text = card.Mana.ToString();
        imageSR.sprite = card.Image;

        SetupTypeText();
        SetupOwnerText();
        ApplyOwnerTint();
    }

    public void RefreshDynamicText()
    {
        if (Card == null) return;

        description.text = GetDynamicDescription(Card);
        mana.text = Card.Mana.ToString();

        SetupTypeText();
        SetupOwnerText();
        ApplyOwnerTint();
    }

    private string GetDynamicDescription(Card card)
    {
        if (card == null || card.Data == null)
            return string.Empty;

        string finalDescription = card.Description;

        if (finalDescription.Contains("{damage}"))
        {
            int displayedDamage = CalculateDisplayedCardDamage(card.Data.IntentValue);
            finalDescription = finalDescription.Replace("{damage}", displayedDamage.ToString());
        }

        return finalDescription;
    }

    private int CalculateDisplayedCardDamage(int baseDamage)
    {
        int finalDamage = baseDamage;

        CombatantView caster = GetCurrentCardCaster();

        if (caster != null && caster.GetStatusEffectStacks(StatusEffectType.BURN) > 0)
        {
            finalDamage = Mathf.FloorToInt(finalDamage * 0.5f);
        }

        return Mathf.Max(0, finalDamage);
    }

    private CombatantView GetCurrentCardCaster()
    {
        if (Card == null || Card.Data == null)
            return null;

        if (Card.Data.OwnerHero != null && HeroSystem.Instance != null)
        {
            foreach (HeroView heroView in HeroSystem.Instance.HeroViews)
            {
                if (heroView == null || !heroView.gameObject.activeSelf) continue;
                if (heroView.HeroInstance == null || heroView.HeroInstance.Data == null) continue;

                if (heroView.HeroInstance.Data == Card.Data.OwnerHero)
                {
                    return heroView;
                }
            }
        }

        return HeroSystem.Instance != null ? HeroSystem.Instance.MainHeroView : null;
    }

    private void SetupTypeText()
    {
        if (cardTypeText == null || Card == null || Card.Data == null) return;

        cardTypeText.text = Card.Data.Type.ToString();
    }

    private void SetupOwnerText()
    {
        if (ownerText == null || Card == null || Card.Data == null) return;

        ownerText.text = Card.Data.OwnerHero != null
            ? Card.Data.OwnerHero.name.ToUpper()
            : "BASIC";
    }

    private void ApplyOwnerTint()
    {
        if (Card == null || Card.Data == null) return;

        Color tint = GetOwnerTint(Card.Data.OwnerHero);

        if (tintableBackgroundSR != null)
            tintableBackgroundSR.color = tint;

        if (typeBannerSR != null)
            typeBannerSR.color = tint;
    }

    private Color GetOwnerTint(HeroData ownerHero)
    {
        if (ownerHero == null)
            return new Color(0.93f, 0.91f, 0.84f, 1f);

        string heroName = ownerHero.name.ToLower();

        if (heroName.Contains("bulba") || heroName.Contains("ivy") || heroName.Contains("venu"))
            return new Color(0.82f, 0.93f, 0.82f, 1f);

        if (heroName.Contains("char") || heroName.Contains("flare") || heroName.Contains("fire"))
            return new Color(0.96f, 0.84f, 0.80f, 1f);

        if (heroName.Contains("squirt") || heroName.Contains("wart") || heroName.Contains("blast"))
            return new Color(0.82f, 0.89f, 0.97f, 1f);

        if (heroName.Contains("pikachu") || heroName.Contains("raichu") || heroName.Contains("electric"))
            return new Color(0.97f, 0.93f, 0.75f, 1f);

        if (heroName.Contains("gastly") || heroName.Contains("haunter") || heroName.Contains("gengar"))
            return new Color(0.87f, 0.82f, 0.95f, 1f);

        switch (Card.Data.Type)
        {
            case CardType.ATTACK:
                return new Color(0.96f, 0.85f, 0.82f, 1f);

            case CardType.SKILL:
                return new Color(0.82f, 0.90f, 0.97f, 1f);

            case CardType.POWER:
                return new Color(0.91f, 0.84f, 0.97f, 1f);

            default:
                return new Color(0.93f, 0.91f, 0.84f, 1f);
        }
    }

    void OnMouseEnter()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;

        RefreshDynamicText();

        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;

        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;

        RefreshDynamicText();

        if (Card.ManualTargetEffect != null)
        {
            ManualTargetSystem.Instance.StartTargeting(transform.position);
        }
        else
        {
            Interactions.Instance.PlayerIsDragging = true;
            wrapper.SetActive(true);
            CardViewHoverSystem.Instance.Hide();
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
        }
    }

    void OnMouseDrag()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (Card.ManualTargetEffect != null) return;

        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }

    void OnMouseUp()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;

        if (Card.ManualTargetEffect != null)
        {
            CombatantView target = ManualTargetSystem.Instance.EndTargeting(MouseUtil.GetMousePositionInWorldSpace(-1));
            if (target != null && ManaSystem.Instance.HasEnoughMana(Card.Mana))
            {
                PlayCardGA playCardGA = new(Card, target);
                ActionSystem.Instance.Perform(playCardGA);
            }
        }
        else
        {
            if (ManaSystem.Instance.HasEnoughMana(Card.Mana)
                && Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f, dropLayer))
            {
                PlayCardGA playCardGA = new(Card);
                ActionSystem.Instance.Perform(playCardGA);
            }
            else
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }

            Interactions.Instance.PlayerIsDragging = false;
        }
    }
}