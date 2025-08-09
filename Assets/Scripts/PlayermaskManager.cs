using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using utils;

public class PlayermaskManager : MonoBehaviour
{
    public void Start()
    {
        GameObject player1 = LoadPrefab.Load("ally/rangeally/r2");
        if (player1 != null)
        {
            // Instantiate the hero prefab in the correct position for battle
            GameObject heroInstance = Instantiate(player1);
            
            // Position hero correctly using BattleGridManager.heroLocation if available
            if (BattleGridManager.Instance != null && BattleGridManager.Instance.heroLocation != null)
            {
                heroInstance.transform.position = BattleGridManager.Instance.heroLocation.position;
                Debug.Log($"[PlayermaskManager] Positioned hero at heroLocation: {heroInstance.transform.position}");
            }
            else
            {
                // Fallback to a reasonable on-screen position
                heroInstance.transform.position = new Vector3(-8.0f, 0.0f, 0); // Left side of screen, center vertically
                Debug.Log($"[PlayermaskManager] Positioned hero at fallback position: {heroInstance.transform.position}");
            }
            
            heroInstance.transform.rotation = Quaternion.identity; // No rotation
            heroInstance.transform.localScale = Vector3.one; // Default scale
            
            // Set sorting order to ensure visibility
            SpriteRenderer sr = heroInstance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10; // Higher than background elements
                sr.sortingLayerName = "Default";
                Debug.Log($"[PlayermaskManager] Set hero sorting order to {sr.sortingOrder}");
            }
        }
        else
        {
            Debug.LogError("[PlayermaskManager] Failed to load r2 hero prefab!");
        }
    }
    
}