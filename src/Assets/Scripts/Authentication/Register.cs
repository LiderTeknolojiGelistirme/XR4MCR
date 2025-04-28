using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField UsernameInput;
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public TMP_InputField FirstNameInput;
    public TMP_InputField LastNameInput;
    public Button RegisterButton;
    public TMP_Text MessageText;

    private const string KeycloakUrl = "http://localhost:8080"; // Keycloak URL
    private const string Realm = "myrealm"; // Realm ad�
    private const string AdminClientId = "admin-cli"; // Admin client ID
    private const string AdminUsername = "admin"; // Admin kullan�c� ad�
    private const string AdminPassword = "admin"; // Admin �ifre

    private void Start()
    {
        RegisterButton.onClick.AddListener(async () => await Register());
    }

    private async Task Register()
    {
        try
        {
            var username = UsernameInput.text;
            var email = EmailInput.text;
            var password = PasswordInput.text;
            var firstName = FirstNameInput.text;
            var lastName = LastNameInput.text;

            var adminToken = await GetAdminTokenAsync();
            await CreateUserAsync(adminToken, username, email, password, firstName, lastName);
        }
        catch (Exception ex)
        {
            MessageText.text = $"Error: {ex.Message}";
        }
    }

    private async Task<string> GetAdminTokenAsync()
    {
        using var client = new HttpClient();

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", AdminClientId),
            new KeyValuePair<string, string>("username", AdminUsername),
            new KeyValuePair<string, string>("password", AdminPassword),
            new KeyValuePair<string, string>("grant_type", "password")
        });

        var response = await client.PostAsync($"{KeycloakUrl}/realms/master/protocol/openid-connect/token", content);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(json)["access_token"]?.ToString();
            return token;
        }
        else
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Failed to retrieve admin token. Details: {errorDetails}");
            throw new Exception("Failed to retrieve admin token.");
        }
    }

    private async Task CreateUserAsync(string adminToken, string username, string email, string password, string firstName, string lastName)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var newUser = new
        {
            username = username,
            email = email,
            firstName = firstName,
            lastName = lastName,
            enabled = true,
            emailVerified = true, // Email do�rulama durumu
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false } // Temporary false
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{KeycloakUrl}/admin/realms/{Realm}/users", content);

        if (response.IsSuccessStatusCode)
        {
            MessageText.text = "User registered successfully!";
        }
        else
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Failed to create user. Details: {errorDetails}");
            throw new Exception("Failed to create user.");
        }
    }
}
