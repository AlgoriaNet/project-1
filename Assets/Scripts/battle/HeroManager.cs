using System.Collections;
using battle;
using entity;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

public class HeroManager : MonoBehaviour
{
    float time=0;
    [Header("Manager Controller")]
    private SkeletonAnimation _animationController;

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
    [Header("playanimationname")]
    public string idle_name = null;
    public string dead_name = null;
    public string attack_name = null;
    public string wark_name = null;
    public Hero hero;

    private MonsterManager _closestMonster;

    public float attacktime = 0f;
    private IEnumerator _enumerator;


    void Start()
    {
        _enumerator = ShootingDelay();
        _shootingDelay = 1 / attackSpeed;
        _animationController.AnimationState.Event += HandleAnimationEvent;
        attacktime = _animationController.AnimationState.Data.SkeletonData.FindAnimation(attack_name).Duration;
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
        _animationController = GetComponentInChildren<SkeletonAnimation>();
        _animationController.AnimationState.SetAnimation(0, idle_name, true);
    }



    private IEnumerator ShootingDelay()
    {
        _isShooting = true;
        _animationController.AnimationState.SetAnimation(0, attack_name, true);
        if (_animationController.AnimationState.Data.SkeletonData.FindAnimation(attack_name).Duration < 0.4f)
        {
            _animationController.AnimationState.TimeScale = 0.15f;
        }
        yield return new WaitForSeconds(_shootingDelay);
        _isShooting = false;
        _animationController.AnimationState.SetAnimation(0, idle_name, true);
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
    private void HandleAnimationEvent(TrackEntry trackEntry, Event e)
    {
        Debug.Log("Animation Event: " + e.Data.Name);
        // if (e.Data.Name.Equals("hit"))
    }
}
