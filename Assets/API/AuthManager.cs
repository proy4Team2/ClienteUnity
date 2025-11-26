using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;
using System;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    public string CurrentIdToken { get; private set; }

    private FirebaseAuth _auth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _auth = FirebaseAuth.DefaultInstance;
    }

    public async void AuthenticateUser(string email, string password, Action<bool, string> callback)
    {
        try
        {
            var authResult = await _auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser newUser = authResult.User;
            Debug.Log($"Firebase User Login: {newUser.UserId}");
            await GetTokenAsync(newUser, callback);
        }
        catch (Exception ex)
        {
            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            Debug.LogError($"Auth Error: {msg}");
            callback?.Invoke(false, msg);
        }
    }

    private async Task GetTokenAsync(FirebaseUser user, Action<bool, string> callback)
    {
        try
        {
            string token = await user.TokenAsync(true);
            
            CurrentIdToken = token;
            callback?.Invoke(true, "Authentication successful");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Token Error: {ex.Message}");
            callback?.Invoke(false, "Failed to retrieve ID Token");
        }
    }
}