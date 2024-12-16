using model;
using UnityEngine;

namespace battle.test
{
    public class ShowAllSidekicks : MonoBehaviour
    {
        public GameObject prefab;
        private string[] sidekicks = {"Engineer","Blazing_Alchemist","Crystal_Wolf","Dart_Master","Earth_Goelm","Fire_Hawk","Flame_Spirit","Frost_Mage","Glacial_Bear","Golden_Crane","Holy_Cleric","Ice_Spirit","Knight","Light_Spirit","Main_Hero","Pyromancer","Shadow_Panther","Shadow_Spirit","Storm_Caller","Wind_Fox","Wind_Spirit",};
        private void Start()
        {
            Vector2 topLeft = BattleGridManager.Instance.topLeft.position;
            Vector2 bottomRight = BattleGridManager.Instance.bottomRight.position;
            Vector2 size = bottomRight - topLeft;
            float width = size.x / 5;
            float height = size.y / 4;
            for (int i = 0; i < sidekicks.Length; i++)
            {
                int row = i / 5;
                int col = i % 5;
                Vector2 position = new Vector2(topLeft.x + width * col + width / 2, topLeft.y + height * row + height / 2);
               
                GameObject sidekickObj = Instantiate(prefab, position, Quaternion.identity);
                var sidekickManager = sidekickObj.GetComponent<SidekickManager>();
                var sidekick = new Sidekick
                {
                    Name = sidekicks[i],
                    Hp = 100,
                    Atk = 40,
                    Skill = new Skill
                    {
                        Name = "Skill2",
                        Icon = "skill_icon_117",
                        Duration = 10f,
                        ReleaseCount = 3,
                        Cd = 5,
                        DamageType = DamageType.Light,
                        SkillTargetType = SkillTargetType.LatestMultiple,
                        IsDynamic = true,
                    }
                };
                sidekickManager.Init(sidekick);
            }
        }
    }
}