namespace battle
{
    public class SkillLevelUpUtils
    {
        public static void ExtraDamageGain(Skill skill, float value)
        {
            skill.ExtraDamageGain += value;
        }
        
        public static void AddDuration(Skill skill, float value)
        {
            skill.Duration += value;
        }
        
        public static void ReduceCd(Skill skill, float value)
        {
            skill.Cd -= value;   
        }

        public static void AddScope(Skill skill, float value)
        {
            skill.Scope += value;
        }
        
        public static void AddReleaseCount(Skill skill, int value)
        {
            skill.ReleaseCount += value;
        }
        
        public static void AddLaunchesCount(Skill skill, int value)
        {
            skill.LaunchesCount += value;
        }
        
        public static void ActiveCharacter(Skill skill, string character)
        {
            skill.ActiveCharacter.Add(character);
        }   
    }
}