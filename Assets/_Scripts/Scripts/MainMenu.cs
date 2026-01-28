using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsMenu;
    public GameObject mainMenu;
    public GameObject loginPanel;
    public GameObject accountPanel;

    [Header("Fancy Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Buttons")]
    public Button playButton;
    public Button loginButton;
    public TMP_Text loginButtonText; 

    [Header("Audio")]
    [SerializeField] private AudioClip backgroundMusic;

    private void Start()
    {
        // Debug Music Reference
        string path = GetPath(gameObject);
        if (backgroundMusic == null) Debug.LogWarning($"[MainMenu at {path}] Background Music is NOT assigned in the Inspector!");
        else Debug.Log($"[MainMenu at {path}] Background Music assigned: {backgroundMusic.name}");

        // Start Music
        if (MusicManager.Instance != null && backgroundMusic != null)
        {
            MusicManager.Instance.PlayMusic(backgroundMusic);
        }

        // NEW: Apply fancy effects to all existing buttons
        ApplyFancyEffects();

        // Subscribe to Auth events
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnLoginSuccess += HandleLogin;
            AuthManager.Instance.OnLogout += HandleLogout;
            AuthManager.Instance.OnInitialized += UpdatePlayButton;
            
            if (AuthManager.Instance.IsInitialized)
            {
                ValidateReferences();
                UpdatePlayButton();
                if (!AuthManager.Instance.IsLoggedIn)
                {
                    OpenLoginPanel();
                }
            }

            if (loginButton != null)
            {
                loginButton.onClick.AddListener(Logout);
            }
        }
        else
        {
            Debug.LogError("[MainMenu] AuthManager.Instance not found!");
        }
    }

    private void ApplyFancyEffects()
    {
        // Add floating effect to title if it exists (searching text in parent or nearby)
        TMP_Text title = GetComponentInChildren<TMP_Text>();
        if (title != null && title.gameObject.name.ToLower().Contains("title"))
        {
            if (title.gameObject.GetComponent<UITitleFancy>() == null)
            {
                title.gameObject.AddComponent<UITitleFancy>();
            }
        }

        // Add button effects to all buttons in the canvas
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (var btn in allButtons)
        {
            if (btn.gameObject.GetComponent<UIButtonFancy>() == null)
            {
                btn.gameObject.AddComponent<UIButtonFancy>();
            }
        }
    }

    private void OnDestroy()
    {
        if (AuthManager.HasInstance)
        {
            AuthManager.Instance.OnLoginSuccess -= HandleLogin;
            AuthManager.Instance.OnLogout -= HandleLogout;
            AuthManager.Instance.OnInitialized -= UpdatePlayButton;
        }

        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(Logout);
        }
    }

    private void HandleLogin(Firebase.Auth.FirebaseUser user)
    {
        UpdatePlayButton();
        OpenMainMenuPanel();
    }


    private void HandleLogout()
    {
        UpdatePlayButton();
    }

    private void UpdatePlayButton()
    {
        if (AuthManager.Instance != null)
        {
            bool loggedIn = AuthManager.Instance.IsLoggedIn;
            
            if (playButton != null) playButton.interactable = true; // Always interactable now as per previous turn
            
            if (loginButton != null)
            {
                loginButton.gameObject.SetActive(true);
            }
            
            if (loginButtonText != null)
            {
                loginButtonText.text = loggedIn ? "Cambiar Cuenta" : "Log-In";
            }

            Debug.Log($"[MainMenu] Auth Status updated. LoggedIn: {loggedIn}. Hide login button if false.");
        }
    }

    public void OpenOptionsPanel()
    {
        gameObject.SetActive(true);
        StartCoroutine(SmoothPanelTransition(mainMenu, optionsMenu));
    }

    public void OpenMainMenuPanel()
    {
        gameObject.SetActive(true);
        GameObject currentPanel = null;
        if (optionsMenu != null && optionsMenu.activeSelf) currentPanel = optionsMenu;
        else if (loginPanel != null && loginPanel.activeSelf) currentPanel = loginPanel;
        else if (accountPanel != null && accountPanel.activeSelf) currentPanel = accountPanel;

        if (mainMenu != null) StartCoroutine(SmoothPanelTransition(currentPanel, mainMenu));
        else if (mainMenu != null) mainMenu.SetActive(true);
    }

    public void OpenAccountPanel()
    {
        if (accountPanel != null)
        {
            gameObject.SetActive(true);
            StartCoroutine(SmoothPanelTransition(mainMenu, accountPanel));
        }
        else
        {
            Debug.LogError("[MainMenu] Account Panel not assigned in Inspector!");
        }
    }

    private IEnumerator SmoothPanelTransition(GameObject from, GameObject to)
    {
        if (from == to) yield break;

        // Ensure "to" has a CanvasGroup for fading
        CanvasGroup toGroup = to.GetComponent<CanvasGroup>();
        if (toGroup == null) toGroup = to.AddComponent<CanvasGroup>();

        // Prep "to" panel (hidden but active)
        toGroup.alpha = 0;
        to.SetActive(true);

        // Fade out "from" panel
        if (from != null && from.activeSelf)
        {
            CanvasGroup fromGroup = from.GetComponent<CanvasGroup>();
            if (fromGroup == null) fromGroup = from.AddComponent<CanvasGroup>();
            
            yield return fromGroup.DOFade(0, fadeDuration).SetUpdate(true).WaitForCompletion();
            
            // CRITICAL: Don't disable the script host itself or transitions will stop!
            if (from != gameObject) from.SetActive(false);
        }

        // Fade in "to" panel
        yield return toGroup.DOFade(1, fadeDuration).SetUpdate(true).WaitForCompletion();
    }

    private string GetPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    public void OpenLoginPanel()
    {
        // 1. Hyper-robust search for the login panel in the scene
        if (loginPanel == null || loginPanel.transform.childCount == 0 || string.IsNullOrEmpty(loginPanel.scene.name))
        {
            Debug.LogWarning($"[MainMenu on {GetPath(gameObject)}] Reference is missing or invalid. Searching scene for any LoginUI...");
            LoginUI[] allLoginUIs = FindObjectsByType<LoginUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            if (allLoginUIs.Length > 0)
            {
                loginPanel = allLoginUIs[0].gameObject;
                Debug.Log($"[MainMenu] Success! Found {allLoginUIs.Length} LoginUI(s). Using '{GetPath(loginPanel)}'");
            }
            else
            {
                // Last ditch effort: Search by name
                GameObject foundByName = GameObject.Find("LoginPopup_Prefab");
                if (foundByName != null)
                {
                    loginPanel = foundByName;
                    Debug.Log($"[MainMenu] Found object by name fallback: '{GetPath(loginPanel)}'");
                }
            }
        }

        // 2. Safety Check: If we STILL don't have a scene instance, DON'T hide the menu
        if (loginPanel == null || string.IsNullOrEmpty(loginPanel.scene.name))
        {
            Debug.LogError($"[MainMenu on {GetPath(gameObject)}] CRITICAL ERROR: Could not find the Login Panel in the scene hierarchy! " +
                "Please ensure the 'LoginPopup_Prefab' is inside your Canvas in the Hierarchy list on the left.");
            return; // Stop here so the menu doesn't disappear
        }

        // 3. Now we are sure we have a valid scene object to show
        GameObject menuToHide = mainMenu != null ? mainMenu : gameObject;
        
        // CRITICAL CHECK: If the login panel is a child of the object we are about to hide, 
        // it will also be hidden! 
        if (loginPanel.transform.IsChildOf(menuToHide.transform))
        {
            Debug.LogError($"[MainMenu] CRITICAL ERROR: The Login Panel '{loginPanel.name}' is a CHILD of '{menuToHide.name}'.");
            // Fallback to basic SetActive to avoid infinite loop or errors in hierarchy
            menuToHide.SetActive(false);
            loginPanel.SetActive(true);
        }
        else
        {
            StartCoroutine(SmoothPanelTransition(menuToHide, loginPanel));
        }

        if (optionsMenu != null && optionsMenu != loginPanel) optionsMenu.SetActive(false);
        
        loginPanel.transform.SetAsLastSibling();
        Debug.Log($"[MainMenu on {GetPath(gameObject)}] Successfully switched to Login Panel: {GetPath(loginPanel)}");
    }

    public void Logout()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.Logout();
            OpenLoginPanel();
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit game action executed");
        Application.Quit();
    }

    public void PlayGame()
    {
        if (AuthManager.Instance.IsLoggedIn)
        {
            SceneManager.LoadScene("CharacterSelection");
        }
        else
        {
            Debug.Log("[MainMenu] Player not logged in. Showing login panel.");
            OpenLoginPanel();
        }
    }
    private void ValidateReferences()
    {
        if (loginPanel != null && string.IsNullOrEmpty(loginPanel.scene.name))
        {
            Debug.LogError($"[MainMenu] ERROR: The 'Login Panel' field in the Inspector is assigned to a PREFAB ASSET ({loginPanel.name}). You MUST drag the object from the HIERARCHY list on the left, not from the Project files.");
            
            // Try fallback: find it in the scene by type
            LoginUI ui = FindFirstObjectByType<LoginUI>(FindObjectsInactive.Include);
            if (ui != null)
            {
                loginPanel = ui.gameObject;
                Debug.Log($"[MainMenu] Fallback: Found {loginPanel.name} in scene. Reference updated automatically.");
            }
        }
    }
}
