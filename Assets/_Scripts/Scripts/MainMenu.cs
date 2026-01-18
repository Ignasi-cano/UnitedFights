using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsMenu;
    public GameObject mainMenu;
    public GameObject loginPanel;
    public GameObject accountPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button loginButton;
    public TMP_Text loginButtonText; // Optional: To change "Login" to "Switch Account"

    private void Start()
    {
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
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void OpenMainMenuPanel()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        else gameObject.SetActive(true);
        
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (accountPanel != null) accountPanel.SetActive(false);
        Debug.Log($"[MainMenu on {GetPath(gameObject)}] Main Menu panel shown.");
    }

    public void OpenAccountPanel()
    {
        if (accountPanel != null)
        {
            accountPanel.SetActive(true);
            if (mainMenu != null) mainMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("[MainMenu] Account Panel not assigned in Inspector!");
        }
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
        // it will also be hidden! We need to warn the user or move it.
        if (loginPanel.transform.IsChildOf(menuToHide.transform))
        {
            Debug.LogError($"[MainMenu] CRITICAL ERROR: The Login Panel '{loginPanel.name}' is a CHILD of '{menuToHide.name}'. " +
                "When we hide the menu, the login panel disappears too! " +
                "FIX: In the Hierarchy, drag '{loginPanel.name}' OUT of '{menuToHide.name}' so they are siblings.");
            
            // Temporary fix: just hide the components instead of the whole object
            // or just don't hide the parent if we want it to work immediately
        }
        else
        {
            menuToHide.SetActive(false);
        }

        if (optionsMenu != null) optionsMenu.SetActive(false);
        
        loginPanel.SetActive(true);
        
        // Ensure it's in front of other UI elements in the same parent
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
