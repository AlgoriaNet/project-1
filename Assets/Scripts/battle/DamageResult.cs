using model;

namespace battle
{
    public class DamageResult
    {
        public int Damage { get; set; }
        public bool IsCrit { get; set; }
        public DamageType DamageType { get; set; }
        public DamageResult(DamageType damageType, int damage, bool isCrit)
        {
            Damage = damage;
            IsCrit = isCrit;
            DamageType = damageType;
        }
    }
}