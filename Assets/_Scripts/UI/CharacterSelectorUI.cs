using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectorUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Transform heroesContainer;
    [SerializeField] private Button heroButtonPrefab; // Prefab with an Image component
    
    private void Start()
    {
        startButton.interactable = false;
        startButton.onClick.AddListener(OnStartGame);
        
        GenerateHeroButtons();
    }

    private void GenerateHeroButtons()
    {
        // Clear existing (if any)
        foreach (Transform child in heroesContainer)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager is missing!");
            return;
        }

        foreach (var hero in GameManager.Instance.AvailableHeroes)
        {
            Button btn = Instantiate(heroButtonPrefab, heroesContainer);
            
            // Try to find the Image component to update.
            // 1. Check if the button itself has the target image (often the 'Target Graphic')
            // 2. Or check if there's a child called "Image" or "Icon"
            // 3. Fallback: Get the first Image component found.
            
            Image btnImage = btn.GetComponent<Image>();
            
            // If the button has an image but it's just a background (like a frame), checking children is safer if the hierarchy is Button -> Icon
            // However, for a simple setup, usually the Button IS the Image.
            
            if (hero.Image != null)
            {
               if (btnImage != null)
               {
                   btnImage.sprite = hero.Image;
               }
               else
               {
                   // Try finding in children if the root has no image
                   var childImage = btn.GetComponentInChildren<Image>();
                   if (childImage != null) childImage.sprite = hero.Image;
               }
            }
            
            // Add Debug loop to verify names
            Debug.Log($"Generated button for {hero.name}, sprite: {hero.Image?.name}");

            btn.onClick.AddListener(() => OnHeroSelected(hero));
        }
    }

    private void OnHeroSelected(HeroData hero)
    {
        GameManager.Instance.SelectHero(hero);
        startButton.interactable = true;
    }

    private void OnStartGame()
    {
        SceneManager.LoadScene("unitedfights");
    }
}
