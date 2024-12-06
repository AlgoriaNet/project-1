using System;
using battle;
using entity;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MonsterManager : MonoBehaviour
{
    public SkeletonAnimation _animationController;
    public GameObject effectenergy;
    public Transform UItransfrom;
    public AudioSource Audio;
    public GameObject ice;

    [Range(20, 400), Tooltip("��Χ��20-200��Ѫ��")]
    private float timedata = 0;

    public bool isDead = false;
    [Header("playanimationname")] public string idle_name = null;
    public string dead_name = null;
    public string attack_name = null;
    public string wark_name = null;
    public float attacktime = 1f;

    [Header("check buff time")] private float buffCheckInterval = 0.1f; // 每 0.1 秒检测一次 Buff
    private float buffCheckTimer = 0f;

    public Monster monster = new Monster();
    public bool canMove = true;


    void Start()
    {
        timedata = 0;
        _animationController = GetComponentInChildren<SkeletonAnimation>();
        ChangeAnimation(wark_name);
        if(_animationController != null)
            attacktime = _animationController.AnimationState.Data.SkeletonData.FindAnimation(attack_name).Duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        buffCheckTimer += Time.fixedDeltaTime;
        if (buffCheckTimer >= buffCheckInterval)
        {
            buffCheckTimer -= buffCheckInterval; // 减去间隔时间，支持累计误差
            monster.BuffManager.TickAllBuffs(buffCheckInterval, monster, this.gameObject);
        }

        CheckBoundary();
        if (monster.IsFrozen || !canMove) return;
        transform.position -= new Vector3(0, monster.Speed * Time.deltaTime / 100, 0);
    }

    private  void CheckBoundary()
    {
        BattleGridManager.Instance.TryReflectBulletIfOutOfBounds(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Finish"))
        {
            Audio.Play();
            ChangeAnimation(attack_name);
        }

        if (collision.gameObject.CompareTag("Bullet"))
        {
            var attacker = collision.gameObject.GetComponent<Attacker>();
            if (attacker == null) return;
            var damageResult = attacker.Living.Attack(DamageType.Mechanical, monster);
            BeHarmed(damageResult);
            CheckDead();
            Destroy(collision.gameObject);
            // Audio.Play();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag($"Bolt"))
        {
            timedata += Time.deltaTime;
            if (timedata > 1.5f)
            {
                Audio.Play();
                BeHarmed(new DamageResult(DamageType.Light, 0, false));
                if (monster.Hp <= 0)
                {
                    ChangeAnimation(dead_name);
                }

                timedata = 0;
            }
        }

        if (other.CompareTag("Spiner"))
        {
            timedata += Time.deltaTime;
            if (timedata > 1.5f)
            {
                Audio.Play();
                BeHarmed(new DamageResult(DamageType.Cure, 0, false));
                if (monster.Hp <= 0)
                {
                    ChangeAnimation(dead_name);
                }

                timedata = 0;
            }
        }

        if (other.CompareTag("Finish"))
        {
            timedata += Time.deltaTime;
            if (timedata > attacktime + 1f)
            {
                var rampart = BattleManager.Instance.Rampart;
                if (rampart == null) return;
                var attackDamage = monster.Attack(DamageType.Physics, rampart);
                BattleManager.Instance.ReduceHp(attackDamage.Damage);
                timedata = 0;
            }
        }
    }

    private void ChangeAnimation(string animationName)
    {
        if (_animationController == null)
            return;
        _animationController.AnimationState.SetAnimation(0, animationName, animationName != dead_name);
    }

    public Boolean CheckDead()
    {
        if (isDead) return true;
        if (monster.Hp <= 0)
        {
            isDead = true;
            var teng = GetComponents<Collider2D>();
            foreach (var item in teng)
            {
                item.enabled = false;
            }

            GetComponent<Rigidbody2D>().useAutoMass = true;
            GetComponent<Rigidbody2D>().simulated = false;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            var game = Instantiate(effectenergy, transform.position, effectenergy.transform.rotation);
            Destroy(game.gameObject, 0.5f);
            BattleManager.Instance.UpdateExperience(monster.Exp);
            Destroy(this.gameObject);
            ChangeAnimation(dead_name);
            return true;
        }

        return false;
    }

    public void BeHarmed(DamageResult damageResult)
    {
        DamagePresentationManager.Instance.ShowDamage(damageResult, UItransfrom);
    }
}