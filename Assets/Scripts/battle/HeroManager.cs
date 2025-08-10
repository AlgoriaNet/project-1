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
    
    // Gun direction tracking for bullet coordination
    private int currentAnimFrame = 0;
    private Vector2[] gunDirections = {
        new Vector2(0.8f, 0.3f),   // Hero_B-1: Gun points right-up
        new Vector2(0.7f, 0.5f),   // Hero_B-2: Gun points right-up  
        new Vector2(1.0f, 0.0f),   // Hero_B-3: Gun points straight ahead
        new Vector2(-0.7f, 0.5f),  // Hero_B-4: Gun points left-up
        new Vector2(-0.8f, 0.3f)   // Hero_B-5: Gun points left
    };


    void Start()
    {
        _enumerator = ShootingDelay();
        _shootingDelay = 1 / attackSpeed;
        // Sync attack timing with animation timing (0.8s per frame, but Hero_B-3 pauses for extra time)
        attacktime = 0.8f * 3f; // Average time for 3-4 frames considering Hero_B-3 pause
    }

    // Update is called once per frame
    void Update()
    {
        // Bullet firing is now handled directly in the animation coroutine
        // This Update method is kept for potential future use
    }
    
    private void Awake()
    {
        hero = new Hero()
        {
            Atk = 50,
            AtkBonus = 4,
            Cri = 50,
        };
        _animationController = GetComponent<Animator>();
        heroBack = GetComponent<SpriteRenderer>();
        
        // Load hero back sprite and play idle animation
        if (heroBack != null)
        {
            // Use correct direct path
            string spritePath = "Sidekicks/Back/Main_Hero_Back/Hero_B-1";
            
            Sprite loadedSprite = Resources.Load<Sprite>(spritePath);
            if (loadedSprite != null)
            {
                heroBack.sprite = loadedSprite;
                
                // Apply proper scaling (reduced by 10%)
                transform.localScale = new Vector3(0.54f, 0.54f, 1f);
                
                // Set sorting order
                heroBack.sortingOrder = 50;
                
                // Position hero (moved up a bit, but below green bar)
                Vector3 centerLeft = new Vector3(-6f, -2f, 0f);
                transform.position = centerLeft;
            }
            
            // Start the hero back animation
            StartCoroutine(AnimateHeroBack());
        }
        
        if (_animationController != null)
        {
            // DISABLE ALL ANIMATOR - it contains wrong sprites (Flame_Spirit instead of Hero_B)
            _animationController.enabled = false;
        }
    }

    private IEnumerator ShootingDelay()
    {
        _isShooting = true;
        if (_animationController != null && _animationController.enabled && HasAnimationState(attack_name))
        {
            _animationController.Play(attack_name);
            
            // If attack animation is too fast, slow it down
            if (attacktime < 0.4f)
            {
                _animationController.speed = 0.15f;
            }
        }
        else if (_animationController != null && !_animationController.enabled)
        {
            // Animator disabled - using sprite-only animation
        }
        
        yield return new WaitForSeconds(_shootingDelay);
        _isShooting = false;
        
        if (_animationController != null && _animationController.enabled && HasAnimationState(idle_name))
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
            
            // Get gun direction based on current animation frame
            Vector2 gunDirection = gunDirections[currentAnimFrame].normalized;
            
            // Calculate bullet spawn position based on gun direction
            Vector3 bulletSpawnOffset = new Vector3(gunDirection.x * 0.5f, gunDirection.y * 0.5f, 0);
            Vector3 bulletSpawnPos = transform.position + bulletSpawnOffset;
            
            // Target direction (still aims at closest monster but starts from gun position)
            Vector2 targetDirection = (_closestMonster.transform.position - bulletSpawnPos).normalized;
            
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPos, Quaternion.AngleAxis(angle, Vector3.forward));
            Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
            bulletRigidbody.velocity = targetDirection * bulletSpeed;
            
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
    }
    
    // Animate hero back sprites
    private IEnumerator AnimateHeroBack()
    {
        if (heroBack == null)
        {
            yield break;
        }
        
        string[] spriteNames = { "Hero_B-1", "Hero_B-2", "Hero_B-3", "Hero_B-4", "Hero_B-5" };
        int currentFrame = 0;
        bool goingForward = true;
        int pauseCount = 0;
        
        while (true)
        {
            string currentSpriteName = spriteNames[currentFrame];
            string spritePath = $"Sidekicks/Back/Main_Hero_Back/{currentSpriteName}";
            
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                heroBack.sprite = sprite;
            }
            
            // Update current animation frame for bullet coordination
            currentAnimFrame = currentFrame;
            
            // Fire bullet every frame change (including B-3 pause frames)
            if (BattleGridManager.Instance != null && BattleGridManager.Instance.monsters.Count > 0)
            {
                _closestMonster = BattleGridManager.Instance.LatestMonster();
                StartCoroutine(_enumerator);
                Hit();
            }
            
            // Special handling for Hero_B-3 (index 2) - pause for extra frames
            if (currentFrame == 2)
            {
                pauseCount++;
                if (pauseCount < 2) // Stay on Hero_B-3 for 2 extra frames
                {
                    yield return new WaitForSeconds(0.8f);
                    continue;
                }
                else
                {
                    pauseCount = 0; // Reset pause counter
                }
            }
            
            // Move to next frame with ping-pong logic
            if (goingForward)
            {
                currentFrame++;
                if (currentFrame >= spriteNames.Length - 1)
                {
                    goingForward = false;
                }
            }
            else
            {
                currentFrame--;
                if (currentFrame <= 0)
                {
                    goingForward = true;
                }
            }
            
            yield return new WaitForSeconds(0.8f); // 1.25 FPS animation
        }
    }
}
