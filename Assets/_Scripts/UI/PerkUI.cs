using UnityEngine;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour
{
    [SerializeField] private Image image;
    public Perk Perk { get; private set; }
    public void Setup(Perk perk)
    {
        Perk = perk;
        if (image != null) image.sprite = perk.Image;
        else Debug.LogError($"[PerkUI] Image component not assigned on {gameObject.name}!");
    }
}
