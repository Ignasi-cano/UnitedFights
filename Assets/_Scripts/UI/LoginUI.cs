using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private bool loadSceneOnSuccess = true;

    private void Start()
    {
        // Add listeners
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);

        // Subscribe to AuthManager events
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
            AuthManager.Instance.OnLoginFailed += HandleLoginFailed;
            AuthManager.Instance.OnRegisterSuccess += HandleRegisterSuccess;
            AuthManager.Instance.OnRegisterFailed += HandleRegisterFailed;
        }
    }

    private void OnEnable()
    {
        SetStatus("");
    }

    private void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        
        SetStatus("Attempting to login...");
        AuthManager.Instance.Login(email, password);
    }

    private void OnRegisterClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        SetStatus("Attempting to register...");
        AuthManager.Instance.Register(email, password);
    }

    private void HandleLoginSuccess(Firebase.Auth.FirebaseUser user)
    {
        SetStatus($"Login successful! Welcome {user.Email}");
        if (loadSceneOnSuccess)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelection");
        }
    }

    private void HandleLoginFailed(string error)
    {
        SetStatus($"Login failed: {error}");
    }

    private void HandleRegisterSuccess(Firebase.Auth.FirebaseUser user)
    {
        SetStatus($"Registration successful! Account created for {user.Email}");
        if (loadSceneOnSuccess)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelection");
        }
    }

    private void HandleRegisterFailed(string error)
    {
        SetStatus($"Registration failed: {error}");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.Log(message);
    }

    private void OnDestroy()
    {
        if (AuthManager.HasInstance)
        {
            AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
            AuthManager.Instance.OnLoginFailed -= HandleLoginFailed;
            AuthManager.Instance.OnRegisterSuccess -= HandleRegisterSuccess;
            AuthManager.Instance.OnRegisterFailed -= HandleRegisterFailed;
        }

        loginButton.onClick.RemoveListener(OnLoginClicked);
        registerButton.onClick.RemoveListener(OnRegisterClicked);
    }
}
