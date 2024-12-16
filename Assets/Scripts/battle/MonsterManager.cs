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
        if (Monster.IsFrozen || !canMove)
        {
            moveAnimator.speed = 0;
        }
        else
        {
            moveAnimator.speed = 1;
            transform.position -= new Vector3(0, Monster.Speed * Time.deltaTime / 100, 0);
        }
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
        if (collision.gameObject.CompareTag("Finish"))
        {
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

        if (other.CompareTag("Finish"))
        {
            _timedata += Time.deltaTime;
            if (_timedata > AttackTime + 1f)
            {
                var rampart = BattleManager.Instance.Rampart;
                if (rampart == null) return;
                var attackDamage = Monster.Attack(DamageType.Physics, rampart);
                BattleManager.Instance.ReduceHp(attackDamage.Damage);
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