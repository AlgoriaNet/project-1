using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class GemLevelsService
{
    [System.Serializable]
    public class GemLevel
    {
        public int level;
        public string id;
        public string name;
    }

    [System.Serializable]
    public class GemLevelsResponse
    {
        public bool success;
        public GemLevelsData data;
    }

    [System.Serializable]
    public class GemLevelsData
    {
        public GemLevel[] gem_levels;
    }

    private static GemLevel[] cachedGemLevels = null;
    private static Dictionary<string, string> cachedGemNameDict = null;
    private static bool isLoading = false;

    public static IEnumerator LoadGemLevels(Action<GemLevel[]> onSuccess, Action<string> onError)
    {
        // Return cached data if available
        if (cachedGemLevels != null)
        {
            onSuccess?.Invoke(cachedGemLevels);
            yield break;
        }

        // Prevent multiple simultaneous API calls
        if (isLoading)
        {
            // Wait for current loading to complete
            yield return new WaitUntil(() => !isLoading);
            
            if (cachedGemLevels != null)
            {
                onSuccess?.Invoke(cachedGemLevels);
            }
            else
            {
                onError?.Invoke("Failed to load gem levels");
            }
            yield break;
        }

        isLoading = true;

        string url = Config.BaseUrl + "/api/gem-levels";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        isLoading = false;

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                GemLevelsResponse response = JsonConvert.DeserializeObject<GemLevelsResponse>(request.downloadHandler.text);
                
                if (response.success && response.data?.gem_levels != null)
                {
                    cachedGemLevels = response.data.gem_levels;
                    
                    // Build dictionary for easy lookup
                    cachedGemNameDict = new Dictionary<string, string>();
                    foreach (var gemLevel in cachedGemLevels)
                    {
                        cachedGemNameDict[gemLevel.id] = gemLevel.name;
                    }
                    
                    Debug.Log($"[GemLevelsService] Loaded {cachedGemLevels.Length} gem levels from API");
                    onSuccess?.Invoke(cachedGemLevels);
                }
                else
                {
                    Debug.LogError("[GemLevelsService] API returned invalid response structure");
                    onError?.Invoke("Invalid response structure");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GemLevelsService] JSON parsing error: {ex.Message}");
                onError?.Invoke($"JSON parsing error: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"[GemLevelsService] API request failed: {request.error}");
            onError?.Invoke($"API request failed: {request.error}");
        }

        request.Dispose();
    }

    public static Dictionary<string, string> GetGemNameDictionary()
    {
        if (cachedGemNameDict == null)
        {
            Debug.LogWarning("[GemLevelsService] Gem levels not loaded yet - returning empty dictionary");
            return new Dictionary<string, string>();
        }

        return new Dictionary<string, string>(cachedGemNameDict); // Return a copy
    }

    public static string GetGemNameById(string gemId)
    {
        if (cachedGemNameDict == null)
        {
            Debug.LogWarning("[GemLevelsService] Gem levels not loaded yet - cannot get gem name");
            return "Unknown Gem";
        }

        return cachedGemNameDict.ContainsKey(gemId) ? cachedGemNameDict[gemId] : "Unknown Gem";
    }

    public static void ClearCache()
    {
        cachedGemLevels = null;
        cachedGemNameDict = null;
        Debug.Log("[GemLevelsService] Gem levels cache cleared");
    }

    public static bool IsGemDataLoaded()
    {
        return cachedGemLevels != null && cachedGemNameDict != null;
    }
}