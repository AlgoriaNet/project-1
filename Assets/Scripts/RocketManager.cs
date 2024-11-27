using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketManager : Bullet
{
    public GameObject Enemy;
    private AudioSource audio;
    Vector3 pos;
    void Awake()
    {
        var shanghai = BattleManager.Instance.shanghai;
        audio = GetComponent<AudioSource>();
        audio.Play();
        this.gameObject.GetComponent<Rigidbody2D>().AddForce(transform.right * 10);
        if (BattleManager.Instance.monsters.Count==0)
        {
            Destroy(gameObject);
            return;
        }
        Enemy = BattleManager.Instance.monsters[Random.Range(0, BattleManager.Instance.monsters.Count)].gameObject;

        if (Enemy==null)
        {
            Destroy(this.gameObject);
        }
        pos = Enemy.transform.position;
    }
    void Update()
    {
        if (Enemy == null)
        {
            Destroy(this.gameObject);
            return;
        }
        if (Enemy.TryGetComponent<MonsterManager>(out MonsterManager enemy))
        {
            if (enemy.isDead)
            {
                Destroy(this.gameObject);
                return;
            }
        }
        transform.position = Vector2.MoveTowards(this.transform.position, Enemy.transform.position, 8f * Time.deltaTime);
        Vector2 direction = Enemy.transform.position - transform.position;
        float Angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        this.gameObject.GetComponent<Rigidbody2D>().rotation = Angle;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enermy"))
        {
            Destroy(this.gameObject);
        }
    }
}
