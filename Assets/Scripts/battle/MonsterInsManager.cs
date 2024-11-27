using System;
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
            public int[] monsterIds;
            public int times;
            public float frequency;
        }
        public static MonsterInsManager Instant;
        private List<MonsterSetting> _monsterSettings;
        
        public List<Transform> inspos = new();
        [Header("Vectors Controller")] public GameObject SpawenLocalisation;
        private readonly Dictionary<int, GameObject> _monsterGames = new();

        private float _timer;
        private float _times;
        public Boolean isGenerateOver = false;

        private void Awake()
        {
            if (Instant == null) Instant = this;
            else Destroy(this);
        }

        private void Start()
        {
            string settingStr =
                "[{\"monsterIds\":[1],  \"times\": 1000,\"frequency\": 0.2},{\"monsterIds\":[2,3],\"times\": 50, \"frequency\":200},{\"monsterIds\":[1],  \"times\": 50, \"frequency\":0.15},{\"monsterIds\":[1],  \"times\": 10, \"frequency\":0.5}]";

            _monsterSettings = JsonConvert.DeserializeObject<List<MonsterSetting>>(settingStr);
            foreach (MonsterSetting setting in _monsterSettings)
            {
                foreach (int monsterId in setting.monsterIds)
                {
                    if (!_monsterGames.ContainsKey(monsterId))
                    {
                        GameObject monster = LoadPrefab.Load("monster/monster" + monsterId);
                        if (monster != null)
                            _monsterGames[monsterId] = monster;
                    }
                }
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
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
            if (currentSetting != null && _timer >= currentSetting.frequency)
            {
                foreach (var id in currentSetting.monsterIds)
                {
                    GameObject monster = Instantiate(_monsterGames[id], Pos(), _monsterGames[id].transform.rotation);
                    monster.transform.parent = SpawenLocalisation.transform;
                    MonsterManager monsterManager = monster.GetComponent<MonsterManager>();
                    Monster entity = new Monster
                    {
                        Hp = 400,
                        Speed = 50
                    };
                    // entity.BuffManager.AddBuff(new BurnBuff(1000, 1, 15));
                    monsterManager.monster = entity;
                }

                _times++;
                _timer = 0;
            }
        }


        Vector3 Pos()
        {
            if (inspos.Count == 1) return inspos[0].transform.position;

            return new Vector3(
                Random.Range(inspos[0].transform.position.x, inspos[1].transform.position.x),
                inspos[0].transform.position.y,
                inspos[0].transform.position.z
            );
        }
    }
}