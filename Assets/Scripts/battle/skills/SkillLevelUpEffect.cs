using System;
using UnityEngine;
using Object = System.Object;

namespace battle
{
    public class SkillLevelUpEffect
    {
        public string SkillName;
        public string EffectName;
        public string Description;
        public string FunctionName;
        public Object Value;
        public int Weight;
        public string DependCharacter;

        public Sprite LoadIconSprite()
        {
            var skill = SkillLevelUpController.GetSkill(SkillName);
            return skill?.LoadIconSprite();
        }
        
        public SkillLevelUpEffect ShallowCopy()
        {
            return (SkillLevelUpEffect)MemberwiseClone();
        }
        
    }
}