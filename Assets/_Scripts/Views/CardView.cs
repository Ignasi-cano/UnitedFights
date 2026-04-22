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

    // This should be the cream/beige body of the card, NOT the blue mana orb.
    [SerializeField] private SpriteRenderer tintableBackgroundSR;

    // Optional: assign if you also want the small type banner tinted
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
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        imageSR.sprite = card.Image;

        SetupTypeText();
        SetupOwnerText();
        ApplyOwnerTint();
    }

    private void SetupTypeText()
    {
        if (cardTypeText == null || Card == null || Card.Data == null) return;

        cardTypeText.text = Card.Data.Type.ToString();
    }

    private void SetupOwnerText()
    {
        if (ownerText == null || Card == null || Card.Data == null) return;

        if (Card.Data.OwnerHero != null)
        {
            ownerText.text = Card.Data.OwnerHero.name.ToUpper();
        }
        else
        {
            ownerText.text = "BASIC";
        }
    }

    private void ApplyOwnerTint()
    {
        if (Card == null || Card.Data == null) return;

        Color tint = GetOwnerTint(Card.Data.OwnerHero);

        // Only tint the selected background pieces
        if (tintableBackgroundSR != null)
        {
            tintableBackgroundSR.color = tint;
        }

        if (typeBannerSR != null)
        {
            typeBannerSR.color = tint;
        }
    }

    private Color GetOwnerTint(HeroData ownerHero)
    {
        // Neutral fallback for basic / no owner
        if (ownerHero == null)
        {
            return new Color(0.93f, 0.91f, 0.84f, 1f);
        }

        string heroName = ownerHero.name.ToLower();

        // Light tint only, so text and UI stay readable
        if (heroName.Contains("bulba") || heroName.Contains("ivy") || heroName.Contains("venu"))
            return new Color(0.82f, 0.93f, 0.82f, 1f); // greenish

        if (heroName.Contains("char") || heroName.Contains("flare") || heroName.Contains("fire"))
            return new Color(0.96f, 0.84f, 0.80f, 1f); // warm red/orange

        if (heroName.Contains("squirt") || heroName.Contains("wart") || heroName.Contains("blast"))
            return new Color(0.82f, 0.89f, 0.97f, 1f); // blueish

        if (heroName.Contains("pikachu") || heroName.Contains("raichu") || heroName.Contains("electric"))
            return new Color(0.97f, 0.93f, 0.75f, 1f); // yellowish

        if (heroName.Contains("gastly") || heroName.Contains("haunter") || heroName.Contains("gengar"))
            return new Color(0.87f, 0.82f, 0.95f, 1f); // purpleish

        // Generic fallback by card type if no specific owner mapping fits
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