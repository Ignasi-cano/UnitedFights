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
        Debug.Log("[UI] CharacterSelectorUI Start. searching for GameManager...");
        startButton.interactable = false;
        startButton.onClick.AddListener(OnStartGame);
        
        GenerateHeroButtons();
    }

    private void GenerateHeroButtons()
    {
        // AUTO-FIX: Ensure the container has a layout group and FORCE IT TO WORK
        HorizontalLayoutGroup layoutGroup = heroesContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            Debug.Log("[UI AUTO-FIX] Adding missing HorizontalLayoutGroup to HeroesContainer.");
            layoutGroup = heroesContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        // Force settings: Disable "Control Child Size" so buttons keep their Prefab size (160x145)
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 50;
        layoutGroup.childControlWidth = false; 
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        
        // Force the layout to calculate immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(heroesContainer.GetComponent<RectTransform>());

        if (GameManager.Instance == null)
        {
            Debug.LogError("[UI] GameManager.Instance is null! This should not happen due to Singleton lazy-loading.");
            return;
        }

        if (GameManager.Instance.AvailableHeroes == null || GameManager.Instance.AvailableHeroes.Count == 0)
        {
            Debug.LogWarning("[UI] GameManager has NO HEROES in AvailableHeroes! Did you start from the Login scene? Make sure your configured GameManager is in the first scene of the game.");
            return;
        }

        Debug.Log($"[UI] Found {GameManager.Instance.AvailableHeroes.Count} heroes in GameManager.");

        foreach (var hero in GameManager.Instance.AvailableHeroes)
        {
            Button btn = Instantiate(heroButtonPrefab, heroesContainer);
            
            // NEW LOGIC: improved targeting for the User's Prefab structure (HeroButton -> HeroSprite)
            Image targetImage = null;
            
            // 1. Try to find specific child by name ("HeroSprite" or "Icon")
            Transform spriteChild = btn.transform.Find("HeroSprite");
            if (spriteChild == null) spriteChild = btn.transform.Find("Icon");
            
            if (spriteChild != null)
            {
                targetImage = spriteChild.GetComponent<Image>();
            }

            // 2. If no specific child, try to get the image on the button itself
            if (targetImage == null)
            {
                targetImage = btn.GetComponent<Image>();
            }

            // 3. Last resort: Get ANY image in children
            if (targetImage == null)
            {
                targetImage = btn.GetComponentInChildren<Image>();
            }
            
            if (hero.Image != null)
            {
               if (targetImage != null)
               {
                   Debug.Log($"[UI DEBUG] Setting Sprite for '{hero.name}' on Object: '{targetImage.gameObject.name}'.");
                   targetImage.sprite = hero.Image;
                   targetImage.color = Color.white; // Force opacity
                   targetImage.preserveAspect = true; // Keep aspect ratio
               }
               else
               {
                    Debug.LogError($"[UI DEBUG] Could not find ANY Image component to set for hero '{hero.name}'!");
               }
            }
            else
            {
                Debug.LogWarning($"[UI DEBUG] Hero {hero.name} has NO IMAGE assigned in Data!");
            }
            
            // Add Debug loop to verify names
            Debug.Log($"Generating Button for Hero: '{hero.name}' (InstanceID: {hero.GetInstanceID()}) - Sprite: {(hero.Image != null ? hero.Image.name : "NULL")}");

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
        SceneManager.LoadScene("MapScene");
    }
}
