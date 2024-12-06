using battle;
using UnityEngine;
using utils;

public class SkillIceCrackBullet : BaseSkillController
{
    public GameObject reflectBulletPrefab; // 子弹B的预制体
    private Rigidbody2D _bulletRigidbody;   
    public int separateNumber = 5;

    protected override void Start()
    {
        base.Start();
        transform.rotation = Utils.DirectionQuaternion(TargetDirection, Vector2.left);
        _bulletRigidbody = GetComponent<Rigidbody2D>();
        _bulletRigidbody.velocity = TargetDirection * Skill.Speed;
    }

    protected override void WhenAttackAfter()
    {
        SplitIntoBulletB();
    }

    private void SplitIntoBulletB()
    {
        float maxOffset = 50f;
        var directions = Utils.AngleOffsetDirections(TargetDirection, maxOffset, separateNumber);

        for (int i = 0; i < separateNumber; i++)
        {
            GameObject bulletB = Instantiate(reflectBulletPrefab, transform.position, Quaternion.identity);
            SkillSetting skillSetting = bulletB.GetComponent<SkillSetting>();
            skillSetting.Living = Living;
            skillSetting.targetIndex = TargetIndex;
            skillSetting.targetDirection = directions[i];
            Rigidbody2D rb = bulletB.GetComponent<Rigidbody2D>();
            rb.velocity = directions[i] * Skill.Speed;
            bulletB.transform.rotation = Utils.DirectionQuaternion(directions[i], Vector2.left);
        }
    }
}