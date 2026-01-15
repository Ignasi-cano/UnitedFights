using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthManager : Singleton<AuthManager>
{
    private FirebaseAuth auth;
    public FirebaseUser CurrentUser => auth?.CurrentUser;
    public bool IsLoggedIn => CurrentUser != null;
   
    public event Action<FirebaseUser> OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action<FirebaseUser> OnRegisterSuccess;
    public event Action<string> OnRegisterFailed;
    public event Action OnLogout;

    private void Start()
    {
        if (FirebaseManager.Instance.IsInitialized)
        {
            InitializeAuth();
        }
        else
        {
            FirebaseManager.Instance.OnFirebaseInitialized += InitializeAuth;
        }
    }

    private void InitializeAuth()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (CurrentUser != null)
        {
            Debug.Log($"User signed in: {CurrentUser.Email}");
        }
    }

    public void Register(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    string error = GetErrorMessage(task.Exception);
                    OnRegisterFailed?.Invoke(error);
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                OnRegisterSuccess?.Invoke(newUser);
               
                // Create user document in Firestore
                // Ensure ScoreManager is ready or handle this dependency carefully
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.CreateUserDocument(newUser.UserId, email);
                }
                else
                {
                    Debug.LogWarning("ScoreManager instance not found during registration.");
                }
            });
    }

    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    string error = GetErrorMessage(task.Exception);
                    OnLoginFailed?.Invoke(error);
                    return;
                }

                OnLoginSuccess?.Invoke(task.Result.User);
            });
    }

    public void Logout()
    {
        auth?.SignOut();
        OnLogout?.Invoke();
    }

    private string GetErrorMessage(AggregateException exception)
    {
        FirebaseException firebaseEx = exception?.GetBaseException() as FirebaseException;
        return firebaseEx != null
            ? ((AuthError)firebaseEx.ErrorCode).ToString()
            : "Unknown error: " + exception?.Message;
    }

    private void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= OnAuthStateChanged;
    }
}
