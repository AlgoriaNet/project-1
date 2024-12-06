using System.Collections.Generic;
using UnityEngine;

namespace battle
{
    public class SkillLevelUpController
    {
        public List<SkillLevelUpEffect> Effects = new();


        public List<SkillLevelUpEffect> GenerateOption()
        {
            var maxCount = GenerateOptionCount();
            List<SkillLevelUpEffect> options = new();
            for (int i = 0; i < maxCount; i++)
            {
                int weight = 0;
                foreach (var effect in Effects)
                {
                    if (effect.DependCharacter == null ||
                        GetSkill(effect.SkillName).HasCharacter(effect.DependCharacter))
                    {
                        weight += effect.Weight;
                    }
                }

                int random = Random.Range(0, weight);
                for (var i1 = 0; i1 <= Effects.Count - 1; i1++)
                {
                    if (Effects[i1].DependCharacter == null ||
                        GetSkill(Effects[i1].SkillName).HasCharacter(Effects[i1].DependCharacter))
                    {
                        random -= Effects[i1].Weight;
                        if (random <= 0)
                        {
                            options.Add(Effects[i1].ShallowCopy());
                            break;
                        }
                    }
                }
            }
            return options;
        }

        public int GenerateOptionCount()
        {
            int count = 0;
            Debug.Log(Effects.Count);
            foreach (var effect in Effects)
            {
                if (effect.DependCharacter == null) count++;
                else if (GetSkill(effect.SkillName).HasCharacter(effect.DependCharacter)) count++;
            }

            if (count >= 3) return 3;
            return count;
        }

        public static Skill GetSkill(string skillName)
        {
            var sidekick = BattleGridManager.Instance.Sidekicks.Find(v => v.Skill.Name == skillName);
            if (sidekick != null) return sidekick.Skill;
            return null;
        }

        public void ApplyEffect(SkillLevelUpEffect effect)
        {
            var skill = GetSkill(effect.SkillName);
            if (skill == null) return;
            switch (effect.FunctionName)
            {
                case "ExtraDamageGain":
                    SkillLevelUpUtils.ExtraDamageGain(skill, (float)effect.Value);
                    break;
                case "AddDuration":
                    SkillLevelUpUtils.AddDuration(skill, (float)effect.Value);
                    break;
                case "ReduceCd":
                    SkillLevelUpUtils.ReduceCd(skill, (float)effect.Value);
                    break;
                case "AddScope":
                    SkillLevelUpUtils.AddScope(skill, (float)effect.Value);
                    break;
                case "AddReleaseCount":
                    SkillLevelUpUtils.AddReleaseCount(skill, (int)effect.Value);
                    break;
                case "AddLaunchesCount":
                    SkillLevelUpUtils.AddLaunchesCount(skill, (int)effect.Value);
                    break;
                case "ActiveCharacter":
                    SkillLevelUpUtils.ActiveCharacter(skill, (string)effect.Value);
                    break;
            }
            DeleteEffect(effect); 
        }
        public void DeleteEffect(SkillLevelUpEffect effect)
        {
            
            foreach (var _effect in Effects)
            {
                if(_effect.Description == effect.Description)
                {
                    Effects.Remove(_effect);
                    break;
                }
            }
        }
    }
}