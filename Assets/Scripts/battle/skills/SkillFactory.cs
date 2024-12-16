using System.Collections.Generic;
using model;
using UnityEngine;
using utils;

namespace battle
{
    public abstract class SkillFactory
    {
        public static GameObject Create(string skillName, Living living, Skill skill, int targetIndex,
            Vector2 direction, Vector3 position)
        {
            var skillPrefab = Resources.Load<GameObject>(Path.GetPath(Path.SkillPrefab, skillName));
            if (skillPrefab)
            {
                GameObject skillObject = Object.Instantiate(skillPrefab);
                skillObject.transform.position = position;
                SkillSetting setting = skillObject.GetComponent<SkillSetting>();
                setting.Living = living;
                setting.Skill = skill;
                setting.targetIndex = targetIndex;
                setting.targetDirection = direction;
                return skillObject;
            }
            return null;
        }
    }
}