using entity;

namespace battle
{
    public class Skill
    {
        public string Name;
        public string Icon;
        public SkillArea Area;
        public bool IsDurationDamage;
        public float Duration;
        public float Cd;
        public float MultiplyingPower;
        public DamageType DamageType;
        public float AttackFrequency;
        public Buff AdditionalBuff;
    }
}