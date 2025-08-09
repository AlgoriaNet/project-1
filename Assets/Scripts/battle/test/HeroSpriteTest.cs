using UnityEngine;

namespace battle.test
{
    /// <summary>
    /// Test script to verify hero sprite loading without corruption
    /// This helps diagnose sprite loading issues by testing direct paths
    /// </summary>
    public class HeroSpriteTest : MonoBehaviour
    {
        [Header("Test Hero Sprite Loading")]
        public bool testOnStart = true;
        
        void Start()
        {
            if (testOnStart)
            {
                TestHeroSpriteLoading();
            }
        }
        
        [ContextMenu("Test Hero Sprite Loading")]
        public void TestHeroSpriteLoading()
        {
            Debug.Log("[HeroSpriteTest] === TESTING HERO SPRITE LOADING ===");
            
            // Test all 5 hero back sprites
            string[] heroSprites = { "Hero_B-1", "Hero_B-2", "Hero_B-3", "Hero_B-4", "Hero_B-5" };
            
            for (int i = 0; i < heroSprites.Length; i++)
            {
                string spriteName = heroSprites[i];
                string spritePath = $"Sidekicks/Back/Main_Hero_Back/{spriteName}";
                
                Debug.Log($"[HeroSpriteTest] Testing sprite {i + 1}/5: {spriteName}");
                Debug.Log($"[HeroSpriteTest] Loading from path: {spritePath}");
                
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                if (sprite != null)
                {
                    Debug.Log($"[HeroSpriteTest] ✓ SUCCESS: {sprite.name} loaded");
                    Debug.Log($"[HeroSpriteTest]   Texture: {sprite.texture.name}");
                    Debug.Log($"[HeroSpriteTest]   Size: {sprite.rect.width}x{sprite.rect.height}");
                    
                    // Check for flame contamination
                    if (sprite.name.ToLower().Contains("flame") || sprite.texture.name.ToLower().Contains("flame"))
                    {
                        Debug.LogError($"[HeroSpriteTest] ⚠️ CONTAMINATION: Hero sprite contains 'flame' in name!");
                    }
                    else
                    {
                        Debug.Log($"[HeroSpriteTest] ✓ CLEAN: No flame contamination detected");
                    }
                }
                else
                {
                    Debug.LogError($"[HeroSpriteTest] ✗ FAILED to load {spriteName} from {spritePath}");
                }
            }
            
            // Test flame spirit for comparison
            Debug.Log("[HeroSpriteTest] === TESTING FLAME SPIRIT FOR COMPARISON ===");
            string flameSpritePath = "Sidekicks/Back/Flame_Spirit_Back/Flame_Spirit_B-1";
            Sprite flameSprite = Resources.Load<Sprite>(flameSpritePath);
            if (flameSprite != null)
            {
                Debug.Log($"[HeroSpriteTest] Flame Spirit sprite: {flameSprite.name}, Texture: {flameSprite.texture.name}");
            }
            else
            {
                Debug.Log($"[HeroSpriteTest] Flame Spirit sprite not found at {flameSpritePath}");
            }
            
            Debug.Log("[HeroSpriteTest] === TEST COMPLETE ===");
        }
    }
}