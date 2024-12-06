using System;
using entity;
using UnityEngine;
using UnityEngine.Serialization;

namespace battle
{
    public class SkillSetting : MonoBehaviour
    {
        public Living Living;
        public int targetIndex;
        public bool IsDynamic { get;  set; }
        public Vector2 targetDirection;

        private void Start()
        {
            IsDynamic = Living.Skill.IsDynamic;
        }
    }
}