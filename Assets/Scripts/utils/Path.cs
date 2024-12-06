namespace utils
{
    public class Path
    {
        public static string SidekickAttackAnim = "Sidekicks/_#{name}";
        public static string SidekickBackSprite = "Sidekicks/#{name}_Back/#{name}_B-1";
        public static string SkillPrefab = "Skills/Skill_#{name}";
        
        
        public static string GetPath(string key, string name)
        {
            return key.Replace("#{name}", name);
        }
    }
}