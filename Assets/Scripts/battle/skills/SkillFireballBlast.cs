using entity;
using UnityEngine;

namespace battle
{
    public class SkillFireballBlast : BaseSkillController
    {
        protected new int Stage = 2;
        private float _waitTime; 
        [SerializeField] private float duration = 1f;
        
    }
}