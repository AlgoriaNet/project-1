using UnityEngine;
using utils;

namespace battle.BulletTrajectory
{
    public class BulletStraightLine : BaseSkillController
    {
        public bool AdjustmentRotation = true;
        public Vector2 InitialPosition;
        
        protected override void Start()
        {
            base.Start();
            if (AdjustmentRotation) 
                transform.rotation = Utils.DirectionQuaternion(TargetDirection, InitialPosition);
        }
        void Update()
        {
            if (!SkillSetting.IsDynamic) return;
            transform.position +=  (Vector3)TargetDirection * (Skill.Speed * Time.deltaTime);
           
        }
    }
}