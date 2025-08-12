using System;
using battle;
using model;
using Unity.VisualScripting;
using UnityEngine;
using utils;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer monsterSprite;
    [SerializeField] private Animator moveAnimator;
    [SerializeField] private SpriteMask maskSprite;
    [SerializeField] private GameObject effects;
    [SerializeField] private GameObject burnEffect;
    [SerializeField] private GameObject freezeEffect;
    [SerializeField] private GameObject bloodstainEffect;
    [SerializeField] private Transform uiTransform;
    public AudioSource Audio;

    [Range(20, 400), Tooltip("��Χ��20-200��Ѫ��")]
    private float _timedata = 0;

    [HideInInspector] public bool isDead;
    [HideInInspector] public Monster Monster;
    [HideInInspector] public bool canMove = true;
    private const float AttackTime = 1f;
    private const float BuffCheckInterval = 0.1f; // 每 0.1 秒检测一次 Buff
    private float _buffCheckTimer = 0f;
    private const float HitEffectDuration = 0.3f;
    private float _hitEffectWaitTime = 0;


    void Start()
    {
        _timedata = 0;
    }

    public void Init(Monster monster)
    {
        _timedata = 0;
        _hitEffectWaitTime = 0;
        Monster = monster;
        monsterSprite.sprite = Resources.Load<Sprite>(Path.GetPath(Path.MonsterSprite, monster.Name));
        moveAnimator.Play(monster.Name);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        CheckBuffs();
        CheckBoundary();
        CheckMove();
        CheckHitEffect();
    }

    private void CheckMove()
    {
        if (Monster.IsFrozen)
        {
            moveAnimator.speed = 0;
        }
        else
        {
            // Basic logic: just check if we should stop
            // No fancy resume logic for now
            
            if (!canMove)
            {
                moveAnimator.speed = 0;
            }
            else
            {
                // Check if monster should stop based on fence destruction state
                bool shouldStop = ShouldStopAtFence();
                
                if (shouldStop)
                {
                    canMove = false;
                    moveAnimator.speed = 0;
                    Debug.Log($"[MonsterManager] {Monster.Name} stopped at Y={transform.position.y:F2} due to fence state: {FenceDestructionManager.Instance.currentState}");
                }
                else
                {
                    moveAnimator.speed = 1;
                    transform.position -= new Vector3(0, Monster.Speed * Time.deltaTime / 100, 0);
                }
            }
        }
    }

    private bool ShouldStopAtFence()
    {
        if (FenceDestructionManager.Instance == null) return false;
        
        var currentState = FenceDestructionManager.Instance.currentState;
        float currentY = transform.position.y;
        
        // DEBUG: Log every few frames to see monster position and state
        if (Time.frameCount % 60 == 0) // Every 60 frames
        {
            Debug.Log($"[MonsterManager] {Monster.Name} at Y={currentY:F2}, fence state={currentState}");
        }
        
        // Wood phase: Stop at Y=-7.1 (just before WoodGroup at Y=-7.22)
        if (currentState >= FenceDestructionManager.FenceState.WoodGood && 
            currentState <= FenceDestructionManager.FenceState.WoodDamage3)
        {
            bool shouldStop = currentY <= -6.8f;
            if (shouldStop)
            {
                Debug.Log($"[MonsterManager] {Monster.Name} SHOULD STOP at Y={currentY:F2} (wood phase, target Y=-7.1)");
            }
            return shouldStop;
        }
        
        // Steel phase: Stop at Y=-7.8 (just before SteelGroup at Y=-7.92)
        if (currentState >= FenceDestructionManager.FenceState.SteelGood && 
            currentState <= FenceDestructionManager.FenceState.SteelDamage3)
        {
            bool shouldStop = currentY <= -7.3f;
            if (shouldStop)
            {
                Debug.Log($"[MonsterManager] {Monster.Name} SHOULD STOP at Y={currentY:F2} (steel phase, target Y=-7.8)");
            }
            return shouldStop;
        }
        
        // Fence completely destroyed - monsters can pass through
        return false;
    }

    private bool ShouldResumeMovement()
    {
        if (FenceDestructionManager.Instance == null) return false;
        
        var currentState = FenceDestructionManager.Instance.currentState;
        float currentY = transform.position.y;
        
        // If wood is destroyed but we're still at wood position, move closer to steel
        // Only resume if we're clearly above the steel position (not at or below it)
        if (currentState >= FenceDestructionManager.FenceState.SteelDamage1 && currentY > -7.5f)
        {
            Debug.Log($"[MonsterManager] {Monster.Name} should resume: state={currentState}, Y={currentY:F2}");
            return true;
        }
        
        return false;
    }

    private void CheckBuffs()
    {
        _buffCheckTimer += Time.deltaTime;
        if (!(_buffCheckTimer >= BuffCheckInterval)) return;

        _buffCheckTimer -= BuffCheckInterval; // 减去间隔时间，支持累计误差
        Monster.BuffManager.TickAllBuffs(BuffCheckInterval, Monster, this.gameObject);

        burnEffect.SetActive(Monster.IsBurned);
        freezeEffect.SetActive(Monster.IsFrozen);
    }

    private void CheckBoundary()
    {
        BattleGridManager.Instance.TryReflectBulletIfOutOfBounds(gameObject);
    }

    private void CheckHitEffect()
    {
        _hitEffectWaitTime += Time.deltaTime;
        if (_hitEffectWaitTime < HitEffectDuration)
        {
            maskSprite.sprite = monsterSprite.sprite;
            return;
        };
        monsterSprite.color = new Color(1, 1, 1);
        maskSprite.gameObject.SetActive(false);
        _hitEffectWaitTime = 0;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Finish") || collision.gameObject.name.Contains("FinishLine"))
        {
            Debug.Log($"[MonsterManager] {Monster.Name} reached the finish line!");
            Audio.Play();
        }

        if (collision.gameObject.CompareTag("Bullet"))
        {
            var attacker = collision.gameObject.GetComponent<Attacker>();
            if (attacker == null) return;
            var damageResult = attacker.Living.Attack(DamageType.Mechanical, Monster);
            BeHarmed(damageResult);
            CheckDead();
            Destroy(collision.gameObject);
            // Audio.Play();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Finish") || other.gameObject.name.Contains("FinishLine"))
        {
            _timedata += Time.deltaTime;
            if (_timedata > AttackTime)
            {
                var rampart = BattleManager.Instance.Rampart;
                if (rampart == null) 
                {
                    Debug.LogError("[MonsterManager] Rampart is null!");
                    return;
                }
                var attackDamage = Monster.Attack(DamageType.Physics, rampart);
                Debug.Log($"[MonsterManager] {Monster.Name} attacking rampart for {attackDamage.Damage} damage. Rampart HP: {rampart.Hp}");
                
                // Use fence destruction system if available
                if (FenceDestructionManager.Instance != null)
                {
                    FenceDestructionManager.Instance.OnMonsterAttack(attackDamage.Damage);
                }
                else
                {
                    // Fallback to old HP system
                    BattleManager.Instance.ReduceHp(attackDamage.Damage);
                }
                _timedata = 0;
            }
        }
    }


    public Boolean CheckDead()
    {
        if (isDead) return true;
        if (Monster.Hp <= 0)
        {
            isDead = true;
            var teng = GetComponents<Collider2D>();
            foreach (var item in teng) item.enabled = false;
            GetComponent<Rigidbody2D>().useAutoMass = true;
            GetComponent<Rigidbody2D>().simulated = false;
            Destroy(monsterSprite.gameObject);
            Destroy(maskSprite.gameObject);
            burnEffect.SetActive(false);
            freezeEffect.SetActive(false);
            bloodstainEffect.SetActive(true);
            Destroy(gameObject, 2f);
            BattleManager.Instance.UpdateExperience(Monster.Exp);
            return true;
        }

        return false;
    }

    public void BeHarmed(DamageResult damageResult)
    {
        maskSprite.sprite = monsterSprite.sprite;
        _hitEffectWaitTime = 0;
        monsterSprite.color = new Color(243/255f, 181/255f, 171/255f);
        maskSprite.gameObject.SetActive(true);
        DamagePresentationManager.Instance.ShowDamage(damageResult, uiTransform);
    }
}