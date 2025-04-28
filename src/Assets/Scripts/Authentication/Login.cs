using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Viroo.SceneLoader.Actions;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;
    public Button LoginButton;
    public TMP_Text MessageText;

    public GameObject LoadSceneAction;

    private const string KeycloakUrl = "http://localhost:8080";
    private const string Realm = "myrealm";
    private const string ClientId = "myclient";
    private const string ClientSecret = "C7eY2LQTUTUWpoJBR9aPbB69azjAb5Mg";

    private void Start()
    {
        LoginButton.onClick.AddListener(async () => await Login());
    }

    private async Task Login()
    {
        try
        {
            var username = UsernameInput.text.Trim();
            var password = PasswordInput.text.Trim();

            // Empty username check
            if (string.IsNullOrEmpty(username))
            {
                MessageText.text = "Please enter your username.";
                return;
            }

            // Empty password check
            if (string.IsNullOrEmpty(password))
            {
                MessageText.text = "Please enter your password.";
                return;
            }

            var token = await GetTokenAsync(username, password);
            if (!string.IsNullOrEmpty(token))
            {
                MessageText.text = "Login successful!";
                LoadSceneAction.GetComponent<LoadSceneAction>().LocalExecute();
            }
        }
        catch (HttpRequestException)
        {
            MessageText.text = "Authentication service is not available. Please check if Docker is running.";
        }
        catch (Exception ex)
        {
            MessageText.text = ex.Message;
        }
    }

    private async Task<string> GetTokenAsync(string username, string password)
    {
        using var client = new HttpClient();

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("client_secret", ClientSecret),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("grant_type", "password")
        });

        try
        {
            var response = await client.PostAsync($"{KeycloakUrl}/realms/{Realm}/protocol/openid-connect/token", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var token = JObject.Parse(json)["access_token"]?.ToString();
                return token;
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                var errorJson = JObject.Parse(errorDetails);

                if (errorJson["error"]?.ToString() == "invalid_grant")
                {
                    var errorDescription = errorJson["error_description"]?.ToString().ToLower();
                    if (errorDescription.Contains("invalid user credentials"))
                    {
                        throw new Exception("Invalid username or password.");
                    }
                    else if (errorDescription.Contains("user not found"))
                    {
                        throw new Exception("User not found with this username.");
                    }
                }

                throw new Exception($"Login failed. Status Code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException)
        {
            throw new HttpRequestException("Authentication service is not available. Please check if Docker is running.");
        }
    }
}
