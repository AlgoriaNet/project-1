using System.Collections;
using battle;
using model;
using UnityEngine;
using utils;

public class HeroManager : MonoBehaviour
{
    float time=0;
    [Header("Manager Controller")]
    private Animator _animationController;
    [SerializeField] private SpriteRenderer heroBack;

    [Header("Objects Controller")]
    public GameObject bulletPrefab;

    [Header("Position Manager")]
    public Transform firePoint;

    [Header("float Manager")]
    internal float bulletSpeed = 20f;
    internal float attackSpeed = 2.0f;
    private float _shootingDelay;

    [SerializeField] [Header("Boolean Manager")]
    private bool _isShooting = false;

    private bool isover = false;
    [Header("Animation Settings")]
    public string idle_name = "Hero_Idle";
    public string dead_name = "Hero_Dead";
    public string attack_name = "Hero_Attack_New";
    public string walk_name = "Hero_Walk";
    public Hero hero;

    private MonsterManager _closestMonster;

    public float attacktime = 0f;
    private IEnumerator _enumerator;


    void Start()
    {
        _enumerator = ShootingDelay();
        _shootingDelay = 1 / attackSpeed;
        // Get attack animation clip duration - default to 1.0f if not found
        var attackClip = GetAnimationClip(attack_name);
        attacktime = attackClip != null ? attackClip.length : 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        // Every 2 seconds, log current GameObject status for debugging
        if (Mathf.FloorToInt(time / 2f) > Mathf.FloorToInt((time - Time.deltaTime) / 2f))
        {
            LogCurrentStatus();
        }

        if (!(time > attacktime)) return;
        if (BattleGridManager.Instance.monsters.Count==0) return;
        if (BattleGridManager.Instance.monsters.Count>0)
        {
            _closestMonster = BattleGridManager.Instance.LatestMonster();
            StartCoroutine(_enumerator);
            Hit();
        }
        time = 0;
    }
    
    private void LogCurrentStatus()
    {
        Debug.Log($"[HeroManager] === STATUS CHECK ===");
        Debug.Log($"[HeroManager] GameObject: {gameObject.name}, Active: {gameObject.activeInHierarchy}");
        Debug.Log($"[HeroManager] Position: {transform.position}, Scale: {transform.localScale}");
        Debug.Log($"[HeroManager] Rotation: {transform.rotation.eulerAngles}");
        
        if (heroBack != null)
        {
            Debug.Log($"[HeroManager] SpriteRenderer enabled: {heroBack.enabled}, Color: {heroBack.color}");
            Debug.Log($"[HeroManager] Sorting Layer: {heroBack.sortingLayerName}, Order: {heroBack.sortingOrder}");
            Debug.Log($"[HeroManager] Current Sprite: {(heroBack.sprite != null ? heroBack.sprite.name : "null")}");
            Debug.Log($"[HeroManager] Bounds: {heroBack.bounds}, IsVisible: {heroBack.isVisible}");
            
            // Check if position is reasonable for screen visibility
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Debug.Log($"[HeroManager] Screen Position: {screenPos}");
            
            if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height || screenPos.z < 0)
            {
                Debug.LogWarning($"[HeroManager] GameObject may be OFF-SCREEN! Screen pos: {screenPos}, Screen size: {Screen.width}x{Screen.height}");
            }
            else
            {
                Debug.Log($"[HeroManager] GameObject is ON-SCREEN at {screenPos}");
            }
        }
        else
        {
            Debug.LogError("[HeroManager] heroBack SpriteRenderer is NULL in status check!");
        }
        
        // Check if there are other visual components that might be competing
        var allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        Debug.Log($"[HeroManager] Total SpriteRenderers in children: {allSpriteRenderers.Length}");
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            var sr = allSpriteRenderers[i];
            string spriteName = sr.sprite != null ? sr.sprite.name : "null";
            Debug.Log($"[HeroManager] Child SR {i}: {sr.gameObject.name}, sprite: {spriteName}, enabled: {sr.enabled}, visible: {sr.isVisible}");
        }
        
        // Check for competing SidekickManager components
        var sidekickManagers = GetComponentsInChildren<battle.SidekickManager>();
        Debug.Log($"[HeroManager] Found {sidekickManagers.Length} SidekickManager components");
        for (int i = 0; i < sidekickManagers.Length; i++)
        {
            var sm = sidekickManagers[i];
            Debug.Log($"[HeroManager] SidekickManager {i}: GameObject={sm.gameObject.name}, Active={sm.gameObject.activeInHierarchy}, IsActive={sm.isActive}");
            
            // Check if this sidekick is displaying flame sprites
            var sidekickSR = sm.GetComponent<SpriteRenderer>();
            if (sidekickSR != null && sidekickSR.sprite != null)
            {
                string spriteName = sidekickSR.sprite.name;
                if (spriteName.ToLower().Contains("flame"))
                {
                    Debug.LogError($"[HeroManager] ⚠️ FOUND COMPETING FLAME SIDEKICK: {sm.gameObject.name} showing {spriteName}");
                    Debug.LogError($"[HeroManager] This Flame sidekick may be overlaying the hero!");
                    Debug.LogError($"[HeroManager] Sidekick position: {sm.transform.position}, Scale: {sm.transform.localScale}");
                    Debug.LogError($"[HeroManager] Sidekick sorting layer: {sidekickSR.sortingLayerName}, Order: {sidekickSR.sortingOrder}");
                    
                    // TEMPORARY FIX: Move flame sidekick behind hero
                    if (sidekickSR.sortingOrder >= heroBack.sortingOrder)
                    {
                        sidekickSR.sortingOrder = heroBack.sortingOrder - 1;
                        Debug.LogWarning($"[HeroManager] FIXED: Moved flame sidekick behind hero. New order: {sidekickSR.sortingOrder}");
                    }
                }
            }
        }
        
        // Check parent and sibling components for competing visuals
        var parentSpriteRenderers = GetComponentsInParent<SpriteRenderer>();
        Debug.Log($"[HeroManager] Parent SpriteRenderers: {parentSpriteRenderers.Length}");
        for (int i = 0; i < parentSpriteRenderers.Length; i++)
        {
            var sr = parentSpriteRenderers[i];
            if (sr != heroBack) // Don't log our own component
            {
                string spriteName = sr.sprite != null ? sr.sprite.name : "null";
                Debug.Log($"[HeroManager] Parent/Sibling SR {i}: {sr.gameObject.name}, sprite: {spriteName}, enabled: {sr.enabled}, visible: {sr.isVisible}");
                
                if (spriteName.ToLower().Contains("flame"))
                {
                    Debug.LogError($"[HeroManager] FOUND COMPETING FLAME SPRITE: {sr.gameObject.name} has sprite {spriteName}!");
                }
            }
        }
    }
   
    private void Awake()
    {
        hero = new Hero()
        {
            Atk = 50,
            AtkBonus = 4,
            Cri = 50,
        };
        _animationController = GetComponentInChildren<Animator>();
        heroBack = GetComponent<SpriteRenderer>();
        
        // Load hero back sprite and play idle animation
        if (heroBack != null)
        {
            // Log GameObject details for debugging
            Debug.Log($"[HeroManager] GameObject name: {gameObject.name}, Position: {transform.position}, Scale: {transform.localScale}, Active: {gameObject.activeInHierarchy}");
            Debug.Log($"[HeroManager] SpriteRenderer enabled: {heroBack.enabled}, Color: {heroBack.color}, Sorting Layer: {heroBack.sortingLayerName}, Order: {heroBack.sortingOrder}");
            
            // Use correct direct path instead of corrupted Path.GetPath
            string spritePath = "Sidekicks/Back/Main_Hero_Back/Hero_B-1";
            Debug.Log($"[HeroManager] Loading HERO sprite from corrected path: {spritePath}");
            
            Sprite loadedSprite = Resources.Load<Sprite>(spritePath);
            if (loadedSprite != null)
            {
                heroBack.sprite = loadedSprite;
                Debug.Log($"[HeroManager] SUCCESS: Loaded HERO sprite: {loadedSprite.name}, Texture: {loadedSprite.texture.name}");
                Debug.Log($"[HeroManager] Sprite bounds: {heroBack.bounds}, World position: {transform.position}");
                
                // Apply proper scaling to match monster sizes (monsters are typically 1.0 scale)
                transform.localScale = new Vector3(0.8f, 0.8f, 1f); // Slightly smaller than monsters
                Debug.Log($"[HeroManager] Applied hero scaling: {transform.localScale}");
                
                // Ensure hero is in front of any sidekicks by setting higher sorting order
                heroBack.sortingOrder = 10; // Higher than typical sidekick sorting order
                Debug.Log($"[HeroManager] Set hero sorting order to: {heroBack.sortingOrder}");
            }
            else
            {
                Debug.LogError($"[HeroManager] CRITICAL: Failed to load HERO sprite from path: {spritePath}");
                Debug.LogError($"[HeroManager] This means the Hero_B-1.png file is missing or corrupted!");
            }
            
            // Start the hero back animation
            StartCoroutine(AnimateHeroBack());
        }
        else
        {
            Debug.LogError("[HeroManager] heroBack SpriteRenderer is null!");
        }
        
        if (_animationController != null)
        {
            // Only play idle if the animation state exists
            if (HasAnimationState(idle_name))
            {
                _animationController.Play(idle_name);
            }
            else
            {
                Debug.LogWarning($"Animation state '{idle_name}' not found in animator controller");
            }
        }
    }



    private IEnumerator ShootingDelay()
    {
        _isShooting = true;
        if (_animationController != null && HasAnimationState(attack_name))
        {
            _animationController.Play(attack_name);
            
            // If attack animation is too fast, slow it down
            if (attacktime < 0.4f)
            {
                _animationController.speed = 0.15f;
            }
        }
        else if (_animationController != null)
        {
            Debug.LogWarning($"Animation state '{attack_name}' not found in animator controller");
        }
        
        yield return new WaitForSeconds(_shootingDelay);
        _isShooting = false;
        
        if (_animationController != null && HasAnimationState(idle_name))
        {
            _animationController.speed = 1.0f; // Reset speed
            _animationController.Play(idle_name);
        }
    }

    private void Hit()
    {
        if (_closestMonster != null)
        {
            if (_closestMonster.isDead)
            {
                return;
            }
            firePoint.GetChild(0).gameObject.GetComponent<ParticleSystem>().Play();
            GetComponent<AudioSource>().Play();
            Vector2 direction = _closestMonster.transform.position - firePoint.position;
            direction.Normalize();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));
            Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
            bulletRigidbody.velocity = direction * bulletSpeed;
            bullet.GetComponent<Attacker>().Living = hero;
            Destroy(bullet, 4);
        }
    }
    // Helper method to get animation clip duration
    private AnimationClip GetAnimationClip(string animationName)
    {
        if (_animationController == null || _animationController.runtimeAnimatorController == null)
            return null;
            
        foreach (var clip in _animationController.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
                return clip;
        }
        return null;
    }
    
    // Helper method to check if animation state exists
    private bool HasAnimationState(string stateName)
    {
        if (_animationController == null || _animationController.runtimeAnimatorController == null)
            return false;
            
        // Check if the state exists in any layer
        for (int i = 0; i < _animationController.layerCount; i++)
        {
            if (_animationController.HasState(i, Animator.StringToHash(stateName)))
            {
                return true;
            }
        }
        return false;
    }
    
    // Animation event handler (if needed for future use)
    public void OnAttackHit()
    {
        // This can be called from animation events if needed
        Debug.Log("Hero attack hit event");
    }
    
    // Animate hero back sprites
    private IEnumerator AnimateHeroBack()
    {
        if (heroBack == null)
        {
            Debug.LogError("[HeroManager] AnimateHeroBack: heroBack is null!");
            yield break;
        }
        
        string[] spriteNames = { "Hero_B-1", "Hero_B-2", "Hero_B-3", "Hero_B-4", "Hero_B-5" };
        int currentFrame = 0;
        
        Debug.Log("[HeroManager] Starting HERO back animation with 5 frames (NOT flame sprites!)");
        
        while (true)
        {
            string currentSpriteName = spriteNames[currentFrame];
            // Use direct path to avoid Path.GetPath corruption
            string spritePath = $"Sidekicks/Back/Main_Hero_Back/{currentSpriteName}";
            
            Debug.Log($"[HeroManager] Frame {currentFrame + 1}/5: Loading HERO {currentSpriteName} from clean path: {spritePath}");
            
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                heroBack.sprite = sprite;
                Debug.Log($"[HeroManager] Frame {currentFrame + 1}: SUCCESS - Set HERO sprite {sprite.name}, Texture: {sprite.texture.name}");
                
                // Verify this is actually a hero sprite, not a flame sprite
                if (sprite.name.ToLower().Contains("flame") || sprite.texture.name.ToLower().Contains("flame"))
                {
                    Debug.LogError($"[HeroManager] CRITICAL ERROR: Loaded FLAME sprite instead of Hero sprite! Sprite: {sprite.name}, Texture: {sprite.texture.name}");
                    Debug.LogError($"[HeroManager] This means the Hero sprite files are corrupted or mislabeled!");
                }
                else if (sprite.name.Contains("Hero_B-"))
                {
                    Debug.Log($"[HeroManager] VERIFIED: Correct hero back sprite loaded: {sprite.name}");
                }
                else
                {
                    Debug.LogWarning($"[HeroManager] UNEXPECTED: Loaded sprite with unexpected name: {sprite.name}");
                }
                
                // Ensure proper visibility and scale
                if (!heroBack.isVisible)
                {
                    Debug.LogWarning($"[HeroManager] Hero sprite not visible! Position: {transform.position}, Scale: {transform.localScale}");
                }
            }
            else
            {
                Debug.LogError($"[HeroManager] CRITICAL: Failed to load HERO sprite {currentSpriteName} from {spritePath}");
                Debug.LogError($"[HeroManager] Hero sprite file {currentSpriteName}.png may be missing from Resources/Sidekicks/Back/Main_Hero_Back/!");
            }
            
            // Check if this GameObject and SpriteRenderer are still active and visible
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[HeroManager] GameObject became inactive during animation");
            }
            if (!heroBack.enabled)
            {
                Debug.LogWarning("[HeroManager] SpriteRenderer became disabled during animation");
            }
            
            currentFrame = (currentFrame + 1) % spriteNames.Length;
            yield return new WaitForSeconds(0.1f); // 10 FPS animation
        }
    }
}
