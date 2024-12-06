using System.Collections.Generic;
using System.Numerics;
using entity;
using UnityEngine;

namespace battle
{
    public class Skill
    {
        public string Name;
        public string Icon;
        public string Description;
        
        public int ReleaseCount = 1;
        public int LaunchesCount = 1;
        public int ReleaseUltimateCount = 10;
        public int SplitCount = 3;
        public int MaxAngle = 60;
        
        public int Speed = 20;
        public float DestroyDelay;
        public float LaunchesInterval;
        public float DamageRatio = 1;
        public float TwoStageDamageRatio = 0;
        public float ThreeStageDamageRatio = 0;
        public float ExtraDamageGain;
        public float Duration = 5f;
        public float Cd = 10f;
        public float Scope = 1;

        
        public bool IsDynamic;
        public bool IsLivingPositionRelease;
        public bool IsImpenetrability = true;
        public bool IsTraceMonster = false;
        public bool IsCdRestByReleased = false;

        public DamageType DamageType;
        public SkillTargetType SkillTargetType = SkillTargetType.LatestMultiple;
        public List<string> ActiveCharacter = new();
        
        public bool HasCharacter(string character)
        {
            return ActiveCharacter.Contains(character);
        }
        
        public Sprite LoadIconSprite()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("skills/ICON/skill_icon");
            if (sprites != null && sprites.Length > 0)
            {
                foreach (var sprite in sprites)
                {
                    if (sprite.name == Icon)
                    {
                        return sprite;
                    }
                }
            }
            else
            {
                Debug.LogError("skills/ICON/skill_icon/" + Icon + " not found.");
            }
            return null;
        }
    }
}