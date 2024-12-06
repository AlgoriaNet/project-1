using UnityEngine;
using utils;

namespace battle.BulletTrajectory
{
    public class BulletStraightLine : BaseSkillController
    {
        void Update()
        {
            if (!SkillSetting.IsDynamic) return;
            transform.position +=  (Vector3)TargetDirection * (Skill.Speed * Time.deltaTime);
        }
    }
}