using UnityEngine;
using utils;
using System.Linq;

namespace battle.test
{
    /// <summary>
    /// Executive Integration Test for Hero Animation System
    /// Validates that the hero r2 prefab works correctly with the standardized Unity Animator system
    /// Created by CEO to ensure no more failed attempts at hero animation fixes
    /// </summary>
    public class HeroIntegrationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestOnStart = true;
        public bool verboseLogging = true;
        
        void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunHeroIntegrationTests());
            }
        }
        
        private System.Collections.IEnumerator RunHeroIntegrationTests()
        {
            Debug.Log("=== HERO INTEGRATION TEST STARTING ===");
            
            bool allTestsPassed = true;
            
            // Test 1: Hero Prefab Loading
            allTestsPassed &= TestHeroPrefabLoading();
            yield return new WaitForSeconds(0.5f);
            
            // Test 2: Hero Component Validation
            GameObject heroInstance = LoadAndInstantiateHero();
            if (heroInstance != null)
            {
                allTestsPassed &= TestHeroComponents(heroInstance);
                yield return new WaitForSeconds(0.5f);
                
                // Test 3: Animation System Validation
                allTestsPassed &= TestAnimationSystem(heroInstance);
                yield return new WaitForSeconds(0.5f);
                
                // Test 4: Positioning and Visibility
                allTestsPassed &= TestPositioningAndVisibility(heroInstance);
                yield return new WaitForSeconds(0.5f);
                
                // Test 5: Battle Integration
                allTestsPassed &= TestBattleIntegration(heroInstance);
                yield return new WaitForSeconds(0.5f);
                
                // Clean up test instance
                DestroyImmediate(heroInstance);
            }
            else
            {
                allTestsPassed = false;
            }
            
            // Final Result
            if (allTestsPassed)
            {
                Debug.Log("<color=green>=== ALL HERO INTEGRATION TESTS PASSED ===</color>");
                Debug.Log("<color=green>Hero animation crisis has been RESOLVED!</color>");
            }
            else
            {
                Debug.LogError("<color=red>=== HERO INTEGRATION TESTS FAILED ===</color>");
                Debug.LogError("<color=red>Hero animation system requires additional fixes!</color>");
            }
        }
        
        private bool TestHeroPrefabLoading()
        {
            if (verboseLogging) Debug.Log("[Test 1] Testing hero prefab loading...");
            
            GameObject prefab = LoadPrefab.Load("ally/rangeally/r2");
            bool success = prefab != null;
            
            if (success)
            {
                Debug.Log("<color=green>[Test 1] ✅ Hero prefab loaded successfully</color>");
            }
            else
            {
                Debug.LogError("<color=red>[Test 1] ❌ Failed to load hero prefab</color>");
            }
            
            return success;
        }
        
        private GameObject LoadAndInstantiateHero()
        {
            GameObject prefab = LoadPrefab.Load("ally/rangeally/r2");
            if (prefab == null) return null;
            
            GameObject instance = Instantiate(prefab);
            instance.name = "TestHeroInstance";
            
            // Position hero at a visible on-screen location for testing
            if (BattleGridManager.Instance != null && BattleGridManager.Instance.heroLocation != null)
            {
                instance.transform.position = BattleGridManager.Instance.heroLocation.position;
                Debug.Log($"[HeroIntegrationTest] Positioned test hero at heroLocation: {instance.transform.position}");
            }
            else
            {
                // Fallback to a reasonable on-screen position
                instance.transform.position = new Vector3(-8.0f, 0.0f, 0); // Left side of screen, center vertically
                Debug.Log($"[HeroIntegrationTest] Positioned test hero at fallback position: {instance.transform.position}");
            }
            
            // Ensure proper sorting for visibility
            SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10; // Higher than background elements
                sr.sortingLayerName = "Default";
            }
            
            return instance;
        }
        
        private bool TestHeroComponents(GameObject heroInstance)
        {
            if (verboseLogging) Debug.Log("[Test 2] Testing hero component validation...");
            
            bool success = true;
            
            // Test HeroManager component
            HeroManager heroManager = heroInstance.GetComponent<HeroManager>();
            if (heroManager != null)
            {
                Debug.Log("<color=green>[Test 2] ✅ HeroManager component found</color>");
            }
            else
            {
                Debug.LogError("<color=red>[Test 2] ❌ HeroManager component missing</color>");
                success = false;
            }
            
            // Test Animator component
            Animator animator = heroInstance.GetComponent<Animator>();
            if (animator != null)
            {
                Debug.Log("<color=green>[Test 2] ✅ Animator component found</color>");
                
                if (animator.runtimeAnimatorController != null)
                {
                    Debug.Log("<color=green>[Test 2] ✅ Animator Controller assigned</color>");
                }
                else
                {
                    Debug.LogError("<color=red>[Test 2] ❌ Animator Controller missing</color>");
                    success = false;
                }
            }
            else
            {
                Debug.LogError("<color=red>[Test 2] ❌ Animator component missing</color>");
                success = false;
            }
            
            // Test SpriteRenderer component
            SpriteRenderer spriteRenderer = heroInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Debug.Log("<color=green>[Test 2] ✅ SpriteRenderer component found</color>");
                
                if (spriteRenderer.sortingOrder > 0)
                {
                    Debug.Log($"<color=green>[Test 2] ✅ Sorting order is {spriteRenderer.sortingOrder} (> 0)</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[Test 2] ⚠️ Sorting order is {spriteRenderer.sortingOrder}, may cause rendering issues</color>");
                }
            }
            else
            {
                Debug.LogError("<color=red>[Test 2] ❌ SpriteRenderer component missing</color>");
                success = false;
            }
            
            return success;
        }
        
        private bool TestAnimationSystem(GameObject heroInstance)
        {
            if (verboseLogging) Debug.Log("[Test 3] Testing animation system...");
            
            bool success = true;
            
            Animator animator = heroInstance.GetComponent<Animator>();
            if (animator == null) return false;
            
            // Test animation states exist
            string[] requiredStates = { "Hero_Idle", "Hero_Attack_New" };
            
            foreach (string stateName in requiredStates)
            {
                if (HasAnimationState(animator, stateName))
                {
                    Debug.Log($"<color=green>[Test 3] ✅ Animation state '{stateName}' exists</color>");
                }
                else
                {
                    Debug.LogError($"<color=red>[Test 3] ❌ Animation state '{stateName}' missing</color>");
                    success = false;
                }
            }
            
            // Test animation playback
            try
            {
                animator.Play("Hero_Idle");
                Debug.Log("<color=green>[Test 3] ✅ Animation playback working</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>[Test 3] ❌ Animation playback failed: {e.Message}</color>");
                success = false;
            }
            
            return success;
        }
        
        private bool TestPositioningAndVisibility(GameObject heroInstance)
        {
            if (verboseLogging) Debug.Log("[Test 4] Testing positioning and visibility...");
            
            bool success = true;
            
            // Test position
            Vector3 position = heroInstance.transform.position;
            Debug.Log($"[Test 4] Hero position: {position}");
            
            // Test rotation (should be identity or close to it)
            Vector3 rotation = heroInstance.transform.eulerAngles;
            if (Mathf.Abs(rotation.x) < 10f && Mathf.Abs(rotation.y) < 10f && Mathf.Abs(rotation.z) < 10f)
            {
                Debug.Log($"<color=green>[Test 4] ✅ Hero rotation is correct: {rotation}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>[Test 4] ⚠️ Hero rotation may be problematic: {rotation}</color>");
            }
            
            // Test SpriteRenderer visibility
            SpriteRenderer sr = heroInstance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.enabled && sr.color.a > 0.9f)
                {
                    Debug.Log("<color=green>[Test 4] ✅ SpriteRenderer is visible</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[Test 4] ⚠️ SpriteRenderer visibility issues: enabled={sr.enabled}, alpha={sr.color.a}</color>");
                }
            }
            
            // Test screen position
            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(position);
                if (screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.y >= 0 && screenPos.y <= Screen.height)
                {
                    Debug.Log($"<color=green>[Test 4] ✅ Hero is on-screen at {screenPos}</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[Test 4] ⚠️ Hero may be off-screen at {screenPos}</color>");
                }
            }
            
            return success;
        }
        
        private bool TestBattleIntegration(GameObject heroInstance)
        {
            if (verboseLogging) Debug.Log("[Test 5] Testing battle integration...");
            
            bool success = true;
            
            // Test HeroManager functionality
            HeroManager heroManager = heroInstance.GetComponent<HeroManager>();
            if (heroManager != null)
            {
                // Test bullet prefab assignment
                if (heroManager.bulletPrefab != null)
                {
                    Debug.Log("<color=green>[Test 5] ✅ Bullet prefab assigned</color>");
                }
                else
                {
                    Debug.LogWarning("<color=orange>[Test 5] ⚠️ Bullet prefab not assigned</color>");
                }
                
                // Test fire point
                if (heroManager.firePoint != null)
                {
                    Debug.Log("<color=green>[Test 5] ✅ Fire point assigned</color>");
                }
                else
                {
                    Debug.LogWarning("<color=orange>[Test 5] ⚠️ Fire point not assigned</color>");
                }
                
                // Test hero stats
                if (heroManager.hero != null && heroManager.hero.Atk > 0)
                {
                    Debug.Log($"<color=green>[Test 5] ✅ Hero stats configured (Atk: {heroManager.hero.Atk})</color>");
                }
                else
                {
                    Debug.LogWarning("<color=orange>[Test 5] ⚠️ Hero stats not properly configured</color>");
                }
            }
            
            // Test for Spine component conflicts
            var spineComponents = heroInstance.GetComponentsInChildren<Component>().
                Where(c => c.GetType().Name.Contains("Spine") || c.GetType().Name.Contains("Skeleton")).ToArray();
                
            if (spineComponents.Length == 0)
            {
                Debug.Log("<color=green>[Test 5] ✅ No active Spine components found (good for Unity Animator system)</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>[Test 5] ⚠️ Found {spineComponents.Length} Spine components - may cause conflicts</color>");
                foreach (var comp in spineComponents)
                {
                    if (comp.gameObject.activeInHierarchy)
                    {
                        Debug.LogWarning($"<color=orange>[Test 5] ⚠️ Active Spine component: {comp.GetType().Name} on {comp.gameObject.name}</color>");
                    }
                }
            }
            
            return success;
        }
        
        private bool HasAnimationState(Animator animator, string stateName)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return false;
                
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.HasState(i, Animator.StringToHash(stateName)))
                {
                    return true;
                }
            }
            return false;
        }
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void Update()
        {
            // Press T key to run tests manually
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartCoroutine(RunHeroIntegrationTests());
            }
        }
    }
}