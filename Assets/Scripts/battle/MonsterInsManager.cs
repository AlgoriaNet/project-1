using System;
using System.Collections;
using System.Collections.Generic;
using entity;
using Newtonsoft.Json;
using UnityEngine;
using utils;
using Random = UnityEngine.Random;

namespace battle
{
    public class MonsterInsManager : MonoBehaviour
    {
        
        public class MonsterSetting
        {
            public string[] monsterIds;
            public int times;
            public float frequency;
        }
        public static MonsterInsManager Instant;
        private List<MonsterSetting> _monsterSettings;
        
        [Header("Vectors Controller")] public GameObject SpawenLocalisation;
        private readonly Dictionary<string, GameObject> _monsterGames = new();

        private float _times;
        public Boolean isGenerateOver = false;
        private bool _isformation;

        private void Awake()
        {
            if (Instant == null) Instant = this;
            else Destroy(this);
        }

        private void Start()
        {
            string settingStr =
                "[{\"monsterIds\":[\"Abyss_Warlord\",\"Bone_Imp\",\"Clockwork_Phantom\",\"Creeping_Corpse\",\"Dark_Fang\",\"Elemental_Titan\",\"Evil_Goat\",\"Fire_Fiend\",\"Flaming_Giant\",\"Flying_Dragon\",\"Goblin\",\"Grim_Reaper\",\"Havoc_Goat\",\"Mech_Goblin\",\"Mech_Spider\",\"Scrap_Crawler\",\"Shadow_Bat\",\"Spitting_Bloom\",\"Vampire\",\"Venom_Frog\",], " +
                " \"times\": 1000,\"frequency\": 0.5},{\"monsterIds\":[2,3],\"times\": 50, \"frequency\":200},{\"monsterIds\":[1],  \"times\": 50, \"frequency\":0.15},{\"monsterIds\":[1],  \"times\": 10, \"frequency\":0.5}]";

            _monsterSettings = JsonConvert.DeserializeObject<List<MonsterSetting>>(settingStr);
            foreach (MonsterSetting setting in _monsterSettings)
            {
                foreach (string monsterId in setting.monsterIds)
                {
                    if (!_monsterGames.ContainsKey(monsterId))
                    {
                        GameObject monster = LoadPrefab.Load("monster/M_" + monsterId);
                        if (monster != null)
                            _monsterGames[monsterId] = monster;
                    }
                }
            }
        }

        private void Update()
        {
            MonsterSetting currentSetting = null;
            int timesStep = 0;
            foreach (var setting in _monsterSettings)
            {
                currentSetting = setting;
                timesStep += setting.times;
                if (_times <= timesStep) break;
            }

            if (_times > timesStep) isGenerateOver = true;
            if (isGenerateOver) return;
            if (!_isformation)
                StartCoroutine(InsMonster(currentSetting));

        }

        IEnumerator InsMonster(MonsterSetting currentSetting)
        {
            _isformation = true;
            if (currentSetting != null)
            {
                foreach (var id in currentSetting.monsterIds)
                {
                    GameObject monster = Instantiate(_monsterGames[id], Pos(), _monsterGames[id].transform.rotation);
                    monster.transform.parent = SpawenLocalisation.transform;
                    MonsterManager monsterManager = monster.GetComponent<MonsterManager>();
                    Monster entity = new Monster
                    {
                        Hp = 1000,
                        Atk = 50,
                        Speed = 50,
                        Exp = 1,
                    };
                    monsterManager.monster = entity;
                    yield return new WaitForSeconds(currentSetting.frequency);
                }
                _times++;
            }
            _isformation = false;
        }
        


        Vector3 Pos()
        {
            return new Vector3(
                Random.Range(BattleGridManager.Instance.topLeft.transform.position.x, BattleGridManager.Instance.bottomRight.transform.position.x),
                BattleGridManager.Instance.topLeft.transform.position.y,
                BattleGridManager.Instance.topLeft.transform.position.z
            );
        }
    }
}