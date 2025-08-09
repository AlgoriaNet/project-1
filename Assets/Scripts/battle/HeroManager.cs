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
        
        Debug.Log($"[HeroManager] Awake - Found Animator: {_animationController != null}");
        Debug.Log($"[HeroManager] Awake - Found SpriteRenderer: {heroBack != null}");
        if (_animationController != null)
        {
            Debug.Log($"[HeroManager] Awake - Animator Controller: {_animationController.runtimeAnimatorController?.name}");
        }
        
        // Load hero back sprite and play idle animation
        if (heroBack != null)
        {
            // Use correct direct path
            string spritePath = "Sidekicks/Back/Main_Hero_Back/Hero_B-1";
            
            Sprite loadedSprite = Resources.Load<Sprite>(spritePath);
            if (loadedSprite != null)
            {
                heroBack.sprite = loadedSprite;
                Debug.Log($"[HeroManager] Awake - Initial sprite set to: {loadedSprite.name}");
                
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
            Debug.LogWarning($"[HeroManager] Disabling Animator completely - animations contain Flame_Spirit sprites instead of Hero_B");
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
            Debug.Log($"[HeroManager] Playing {attack_name} animation");
        }
        else if (_animationController != null && !_animationController.enabled)
        {
            Debug.Log("[HeroManager] Animator disabled - using sprite-only animation");
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
            Debug.LogError("[HeroManager] heroBack SpriteRenderer is null!");
            yield break;
        }
        
        string[] spriteNames = { "Hero_B-1", "Hero_B-2", "Hero_B-3", "Hero_B-4", "Hero_B-5" };
        int currentFrame = 0;
        
        while (true)
        {
            string currentSpriteName = spriteNames[currentFrame];
            string spritePath = $"Sidekicks/Back/Main_Hero_Back/{currentSpriteName}";
            
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                // CHECK WHAT'S CURRENTLY SHOWING BEFORE CHANGING
                string currentSprite = heroBack.sprite != null ? heroBack.sprite.name : "NULL";
                
                heroBack.sprite = sprite;
                
                Debug.Log($"[HeroManager] SPRITE CHANGE: {currentSprite} -> {sprite.name}");
                
                // Immediately check if something overrode our change
                yield return new WaitForEndOfFrame();
                string afterChangeSprite = heroBack.sprite != null ? heroBack.sprite.name : "NULL";
                if (afterChangeSprite != sprite.name)
                {
                    Debug.LogError($"[HeroManager] SPRITE HIJACKED! We set {sprite.name} but now showing {afterChangeSprite}");
                }
            }
            else
            {
                Debug.LogError($"[HeroManager] Failed to load sprite: {spritePath}");
            }
            
            currentFrame = (currentFrame + 1) % spriteNames.Length;
            yield return new WaitForSeconds(0.8f); // 1.25 FPS animation (half of 0.4f speed)
        }
    }
}
