using entity;
using UnityEngine;

namespace battle
{
    public class SkillFireballSpellBlast : BaseSkillController
    {
        private float _waitTime; 
        [SerializeField] private float duration = 1f;

        protected override void Start()
        {
           SetSkillSetting(GetComponentInParent<SkillSetting>());
           Init();
           Destroy(gameObject, duration);
        }
    }
}