using System;
using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseManager : PersistentSingleton<FirebaseManager>
{
    public bool IsInitialized { get; private set; }
    public event Action OnFirebaseInitialized;

    protected override void Awake()
    {
        base.Awake();
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                IsInitialized = true;
                OnFirebaseInitialized?.Invoke();
                Debug.Log("Firebase initialized successfully");
                
                var sm = ScoreManager.Instance; 
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }
}
