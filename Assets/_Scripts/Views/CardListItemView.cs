using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardListItemView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text ownerText;

    [Header("Images")]
    [SerializeField] private Image artImage;
    [SerializeField] private Image rarityBanner;
    [SerializeField] private Image backgroundImage; // 👈 NUEVO

    public void Setup(CardData data)
    {
        ForceTMPMaterial();
        if (data == null) return;

        // Title
        if (titleText != null)
            titleText.text = data.name;

        // Mana
        if (manaText != null)
            manaText.text = data.Mana.ToString();

        // Type
        if (typeText != null)
            typeText.text = data.Type.ToString();

        // Description
        if (descriptionText != null)
            descriptionText.text = data.Description;

        // Owner
        if (ownerText != null)
        {
            ownerText.text = data.OwnerHero != null
                ? data.OwnerHero.name.ToUpper()
                : "BASIC";
        }

        // Art
        if (artImage != null)
            artImage.sprite = data.Image;

        // Rarity color
        if (rarityBanner != null)
            rarityBanner.color = GetRarityColor(data.Rarity);

        // Background tint (como CardView)
        if (backgroundImage != null)
            backgroundImage.color = GetOwnerTint(data.OwnerHero);
    }
private void ForceTMPMaterial()
{
    foreach (var text in GetComponentsInChildren<TMPro.TMP_Text>(true))
    {
        if (text.fontSharedMaterial != null)
        {
            text.fontMaterial = text.fontSharedMaterial;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
        text.enabled = true;
    }
}
    private Color GetRarityColor(CardRarity rarity)
    {
        return rarity switch
        {
            CardRarity.Basic => new Color(0.8f, 0.8f, 0.8f),
            CardRarity.Common => Color.white,
            CardRarity.Uncommon => new Color(0.5f, 1f, 0.5f),
            CardRarity.Rare => new Color(0.5f, 0.5f, 1f),
            _ => Color.gray
        };
    }

    private Color GetOwnerTint(HeroData ownerHero)
    {
        if (ownerHero == null)
            return new Color(0.93f, 0.91f, 0.84f, 1f);

        string heroName = ownerHero.name.ToLower();

        if (heroName.Contains("bulba") || heroName.Contains("ivy") || heroName.Contains("venu"))
            return new Color(0.82f, 0.93f, 0.82f, 1f);

        if (heroName.Contains("char") || heroName.Contains("fire"))
            return new Color(0.96f, 0.84f, 0.80f, 1f);

        if (heroName.Contains("squirt") || heroName.Contains("water"))
            return new Color(0.82f, 0.89f, 0.97f, 1f);

        if (heroName.Contains("pikachu") || heroName.Contains("electric"))
            return new Color(0.97f, 0.93f, 0.75f, 1f);

        if (heroName.Contains("gastly") || heroName.Contains("gengar"))
            return new Color(0.87f, 0.82f, 0.95f, 1f);

        return new Color(0.93f, 0.91f, 0.84f, 1f);
    }
}