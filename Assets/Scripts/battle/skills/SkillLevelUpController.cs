using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace battle
{
    public class SkillLevelUpController
    {
        public List<SkillLevelUpEffect> Effects = new();


        public List<SkillLevelUpEffect> GenerateOption()
        {
            var maxSize = GenerateOptionCount();
            List<SkillLevelUpEffect> options = new();
            for (int i = 0; i < maxSize; i++)
            {
                int weight = 0;
                foreach (var effect in Effects)
                {
                    if (effect.DependCharacter == null ||
                        GetSkill(effect.SkillName).HasCharacter(effect.DependCharacter))
                    {
                        weight += effect.Weight * effect.MaxCount;
                    }
                }

                int random = Random.Range(0, weight);
                for (var i1 = 0; i1 <= Effects.Count - 1; i1++)
                {
                    if (Effects[i1].DependCharacter == null ||
                        GetSkill(Effects[i1].SkillName).HasCharacter(Effects[i1].DependCharacter))
                    {
                        random -= Effects[i1].Weight * Effects[i1].MaxCount;
                        if (random <= 0)
                        {
                            options.Add(Effects[i1]);
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
            foreach (var keyValuePair in effect.Effects)
            {
                var functionName = keyValuePair.Key;
                var value = keyValuePair.Value;
                switch (functionName)
                {
                    case "ExtraDamageGain":
                        
                        SkillLevelUpUtils.ExtraDamageGain(skill, float.Parse(value));
                        break;
                    case "AddDuration":
                        SkillLevelUpUtils.AddDuration(skill, float.Parse(value));
                        break;
                    case "ReduceCd":
                        SkillLevelUpUtils.ReduceCd(skill, float.Parse(value));
                        break;
                    case "AddScope":
                        SkillLevelUpUtils.AddScope(skill, float.Parse(value));
                        break;
                    case "AddReleaseCount":
                        SkillLevelUpUtils.AddReleaseCount(skill, int.Parse(value));
                        break;
                    case "AddLaunchesCount":
                        SkillLevelUpUtils.AddLaunchesCount(skill, int.Parse(value));
                        break;
                    case "ActiveCharacter":
                        SkillLevelUpUtils.ActiveCharacter(skill, value);
                        break;
                }
            }
            DeleteEffect(effect); 
        }

        private void DeleteEffect(SkillLevelUpEffect effect)
        {
            foreach (var _effect in Effects.ToList())
            {
                if(_effect == effect)
                {
                    Debug.Log("Delete effect: " + effect.SkillName);
                    if (_effect.MaxCount > 1)
                        _effect.MaxCount--;
                    else
                        Effects.Remove(_effect);
                }
            }
        }
    }
}