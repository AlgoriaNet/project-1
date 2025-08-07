using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class CharacterService
{
    [System.Serializable]
    public class CharacterData
    {
        public int id;
        public string name;
        public string fragment_name;
    }

    [System.Serializable]
    public class CharactersResponse
    {
        public bool success;
        public CharactersData data;
    }

    [System.Serializable]
    public class CharactersData
    {
        public CharacterData[] characters;
    }

    private static CharacterData[] cachedCharacters = null;
    private static bool isLoading = false;

    public static IEnumerator LoadCharacters(Action<CharacterData[]> onSuccess, Action<string> onError)
    {
        // Return cached data if available
        if (cachedCharacters != null)
        {
            onSuccess?.Invoke(cachedCharacters);
            yield break;
        }

        // Prevent multiple simultaneous API calls
        if (isLoading)
        {
            // Wait for current loading to complete
            yield return new WaitUntil(() => !isLoading);
            
            if (cachedCharacters != null)
            {
                onSuccess?.Invoke(cachedCharacters);
            }
            else
            {
                onError?.Invoke("Failed to load characters");
            }
            yield break;
        }

        isLoading = true;

        string url = Config.BaseUrl + "/api/allies";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        isLoading = false;

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                CharactersResponse response = JsonConvert.DeserializeObject<CharactersResponse>(request.downloadHandler.text);
                
                if (response.success && response.data?.characters != null)
                {
                    cachedCharacters = response.data.characters;
                    Debug.Log($"[CharacterService] Loaded {cachedCharacters.Length} characters from API");
                    onSuccess?.Invoke(cachedCharacters);
                }
                else
                {
                    Debug.LogError("[CharacterService] API returned invalid response structure");
                    onError?.Invoke("Invalid response structure");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterService] JSON parsing error: {ex.Message}");
                onError?.Invoke($"JSON parsing error: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"[CharacterService] API request failed: {request.error}");
            onError?.Invoke($"API request failed: {request.error}");
        }

        request.Dispose();
    }

    public static string[] GetCharacterNames()
    {
        if (cachedCharacters == null)
        {
            Debug.LogWarning("[CharacterService] Characters not loaded yet - returning empty array");
            return new string[0];
        }

        return cachedCharacters.Select(c => c.name).ToArray();
    }

    public static string GetBaseIdFromName(string characterName)
    {
        if (cachedCharacters == null)
        {
            Debug.LogWarning("[CharacterService] Characters not loaded yet - cannot get base ID");
            return "0";
        }

        var character = cachedCharacters.FirstOrDefault(c => c.name == characterName);
        if (character != null)
        {
            return character.id.ToString();
        }

        Debug.LogWarning($"[CharacterService] Unknown character name: {characterName}");
        return "0";
    }

    public static void ClearCache()
    {
        cachedCharacters = null;
        Debug.Log("[CharacterService] Character cache cleared");
    }

    public static bool IsCharacterDataLoaded()
    {
        return cachedCharacters != null;
    }
}