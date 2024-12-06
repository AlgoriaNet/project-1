using System.Text;

namespace utils
{
    public abstract class Path
    {
        public static string SidekickAttackAnim = "Sidekicks/_{0}";
        public static readonly string SidekickBackSprite = "Sidekicks/Back/{0}_Back/{0}_B-1";
        public static readonly string SkillPrefab = "Skills/Skill_{0}";
        public static readonly string MonsterPrefab = "Monsters/M_{0}";
        public static readonly string MonsterSprite = "Monsters/AnimationImg/{0}/{0}_2";
        
        
        public static string GetPath(string key, params string[] args)
        {
            StringBuilder sb = new StringBuilder(key);
            for (int i = 0; i < args.Length; i++)
            {
                string placeholder = "{" + i + "}";
                sb.Replace(placeholder, args[i]);
            }
            return sb.ToString();
        }
    }
}