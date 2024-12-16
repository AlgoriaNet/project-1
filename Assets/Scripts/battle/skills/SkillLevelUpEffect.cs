using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace battle
{
    public class SkillLevelUpEffect
    {
        public string Id;
        public string SkillName;
        public string EffectName;
        public string Description;
        public Dictionary<string, string> Effects;
        public int Weight;
        public int MaxCount;
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