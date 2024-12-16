using entity;
using UnityEngine;

namespace battle
{
    public class SkillSetting : MonoBehaviour
    {
        public Living Living;
        public Skill Skill;
        public int targetIndex;
        public Vector2 targetDirection;
        public bool isDestroyAfterDuration = true;
        
        public void Start()
        {
            if(isDestroyAfterDuration) Destroy(gameObject, Skill?.Duration ?? 0);
        }
    }
}