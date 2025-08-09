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
        // First, check if we have a clean Hero object already in the scene
        GameObject existingHero = GameObject.Find("Hero");
        GameObject heroInstance = null;
        
        if (existingHero != null)
        {
            // Use the existing Hero object (user's clean duplicate)
            heroInstance = existingHero;
            Debug.Log("[PlayermaskManager] Found existing Hero object - using it instead of r2 prefab");
            
            // Disable any r2 objects to prevent conflicts
            GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in existingPlayers)
            {
                if (player.name == "r2" || player.name.Contains("r2"))
                {
                    player.SetActive(false);
                    Debug.Log($"[PlayermaskManager] Disabled conflicting player object: {player.name}");
                }
            }
        }
        else
        {
            // Fall back to loading r2 prefab
            GameObject player1 = LoadPrefab.Load("ally/rangeally/r2");
            if (player1 != null)
            {
                // Instantiate the hero prefab in the correct position for battle
                heroInstance = Instantiate(player1);
                Debug.Log("[PlayermaskManager] No Hero object found - using r2 prefab");
            }
        }
        
        if (heroInstance != null)
        {
            
            // Position hero at center bottom, moved down by hero height
            Vector3 heroPosition = new Vector3(0f, -9.5f, 0f); // Center X, lower Y (down by hero height)
            heroInstance.transform.position = heroPosition;
            Debug.Log($"[PlayermaskManager] Positioned hero at center bottom: {heroInstance.transform.position}");
            
            heroInstance.transform.rotation = Quaternion.identity; // No rotation
            heroInstance.transform.localScale = new Vector3(0.27f, 0.27f, 1.0f); // Smaller scale (10% smaller than 0.3f)
            
            // Set sorting order to ensure visibility ABOVE green bar
            SpriteRenderer sr = heroInstance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 100; // Much higher than green bar and all other elements
                sr.sortingLayerName = "Default";
                Debug.Log($"[PlayermaskManager] Set hero sorting order to {sr.sortingOrder} to appear above green bar");
            }
            
            // Ensure Hero has correct Animator component and controller
            Animator heroAnimator = heroInstance.GetComponent<Animator>();
            if (heroAnimator == null)
            {
                heroAnimator = heroInstance.AddComponent<Animator>();
                Debug.Log("[PlayermaskManager] Added Animator component to Hero");
            }
            
            // Always assign the correct HeroAnimatorController (override any wrong controller)
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("Sidekicks/HeroAnimatorController");
            if (controller != null)
            {
                heroAnimator.runtimeAnimatorController = controller;
                Debug.Log("[PlayermaskManager] Assigned correct HeroAnimatorController (overriding any existing controller)");
            }
            else
            {
                Debug.LogError("[PlayermaskManager] Failed to load HeroAnimatorController from Resources");
            }
            
            // Create firePoint for Hero object if missing
            var heroManager = heroInstance.GetComponent<HeroManager>();
            if (heroManager != null && heroManager.firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(heroInstance.transform);
                firePointObj.transform.localPosition = new Vector3(1.0f, 0.5f, 0); // Positioned in front of hero
                
                // Create muzzle flash particle system as child
                GameObject muzzleFlash = new GameObject("MuzzleFlash");
                muzzleFlash.transform.SetParent(firePointObj.transform);
                muzzleFlash.transform.localPosition = Vector3.zero;
                
                // Add ParticleSystem component
                ParticleSystem particles = muzzleFlash.AddComponent<ParticleSystem>();
                var main = particles.main;
                main.startLifetime = 0.2f;
                main.startSpeed = 5.0f;
                main.startSize = 0.5f;
                main.maxParticles = 10;
                main.startColor = Color.yellow;
                
                heroManager.firePoint = firePointObj.transform;
                Debug.Log("[PlayermaskManager] Created firePoint with muzzle flash for hero");
            }
        }
        else
        {
            Debug.LogError("[PlayermaskManager] No Hero object found and failed to load r2 hero prefab!");
        }
    }
    
}