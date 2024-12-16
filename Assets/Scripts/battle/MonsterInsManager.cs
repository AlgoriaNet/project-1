using System;
using System.Collections;
using System.Collections.Generic;
using entity;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using utils;
using Random = UnityEngine.Random;

namespace battle
{
    public class MonsterInsManager : MonoBehaviour
    {
        public class MonsterSetting
        {
            public string[] MonsterIds;
            public int Times;
            public float Frequency;
        }
        public static MonsterInsManager Instant;
        private List<MonsterSetting> _monsterSettings;
        [SerializeField] private GameObject baseMonsterPrefab;
        [FormerlySerializedAs("SpawenLocalisation")] [Header("Vectors Controller")] public Transform spawenLocalisation;
        [HideInInspector]public Boolean isGenerateOver;
        
        private float _times;
        private bool _isFormation;

        private void Awake()
        {
            if (Instant == null) Instant = this;
            else Destroy(this);
        }

        private void Start()
        {
            isGenerateOver = false;
            var settingStr =
                "[{\"monsterIds\":[\"Abyss_Warlord\",\"Bone_Imp\",\"Clockwork_Phantom\",\"Creeping_Corpse\",\"Dark_Fang\",\"Elemental_Titan\",\"Evil_Goat\",\"Fire_Fiend\",\"Flaming_Giant\",\"Flying_Dragon\",\"Goblin\",\"Grim_Reaper\",\"Havoc_Goat\",\"Mech_Goblin\",\"Mech_Spider\",\"Scrap_Crawler\",\"Shadow_Bat\",\"Spitting_Bloom\",\"Vampire\",\"Venom_Frog\",], " +
                " \"times\": 1000,\"frequency\": 1},{\"monsterIds\":[2,3],\"times\": 50, \"frequency\":200},{\"monsterIds\":[1],  \"times\": 50, \"frequency\":0.15},{\"monsterIds\":[1],  \"times\": 10, \"frequency\":0.5}]";

            _monsterSettings = JsonConvert.DeserializeObject<List<MonsterSetting>>(settingStr);
        }

        private void Update()
        {
            MonsterSetting currentSetting = null;
            int timesStep = 0;
            foreach (var setting in _monsterSettings)
            {
                currentSetting = setting;
                timesStep += setting.Times;
                if (_times <= timesStep) break;
            }

            if (_times > timesStep) isGenerateOver = true;
            if (isGenerateOver) return;
            if (!_isFormation)
                StartCoroutine(InsMonster(currentSetting));
        }

        IEnumerator InsMonster(MonsterSetting currentSetting)
        {
            _isFormation = true;
            if (currentSetting != null)
            {
                foreach (var id in currentSetting.MonsterIds)
                {
                    GameObject monster = Instantiate(baseMonsterPrefab, Pos(), Quaternion.identity,  spawenLocalisation);
                    MonsterManager monsterManager = monster.GetComponent<MonsterManager>();
                    Monster entity = new Monster
                    {
                        Name = id,
                        Hp = 200,
                        Atk = 50,
                        Speed = 50,
                        Exp = 10,
                    };
                    monsterManager.Init(entity);
                    
                    monsterManager.Monster = entity;
                    yield return new WaitForSeconds(currentSetting.Frequency);
                }
                _times++;
            }
            _isFormation = false;
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