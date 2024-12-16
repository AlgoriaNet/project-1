using System;
using System.Collections.Generic;
using model;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace battle
{
    public class SidekickIntoBattleManager : MonoBehaviour
    {
        public static SidekickIntoBattleManager Instance { get; private set; }
        public GameObject parent;
        public GameObject skillCdPrefab;
        public GameObject baseSidekickPrefab;
        public List<Transform> sidekickPositions;
        public List<GameObject> skillsCds = new();
        private bool _isInit;

        private void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Update()
        {
            if(_isInit) return;
            var sidekicks = BattleGridManager.Instance.Sidekicks;
            if (sidekicks.Count > 0)
            {
               
                _isInit = true;
                for (int i = 0; i < sidekicks.Count; i++)
                {
                    IntoBattle(i, sidekicks[i]);
                }
            }
        }

        private void IntoBattle(int index, Sidekick sidekick)
        {
            if(index > 3) return;
            try
            {
                var sidekickObj = Instantiate(baseSidekickPrefab,
                    sidekickPositions[index].position,
                    Quaternion.identity,
                    parent.transform);
                var sidekickManager = sidekickObj.GetComponent<SidekickManager>();
                sidekickManager.Init(sidekick);
                sidekickPositions[index].gameObject.SetActive(false);
                SetSkillCd(index, sidekick, sidekickManager);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        private void SetSkillCd(int index, Sidekick sidekick,SidekickManager sidekickManager)
        {
            var skillCd = Instantiate(skillCdPrefab, skillsCds[index].transform, true);
            var rectTransform = skillCd.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero; // 如果你需要在锚点位置
            rectTransform.offsetMin = Vector2.zero; // 确保左下角偏移是0
            rectTransform.offsetMax = Vector2.zero; // 确保右上角偏移是0
            rectTransform.localScale = new Vector3(1, 1, 1);
            
            var image = skillCd.GetComponent<Image>();
            Sprite icon = sidekick.Skill.LoadIconSprite();
            if (icon == null) return;
            image.sprite = icon;
            GameObject mask = skillCd.transform.Find("skillCD_Mask").gameObject;
            mask.GetComponent<Image>().sprite = icon;
            GameObject text = skillCd.transform.Find("skillCD_Text").gameObject;
            sidekickManager.skillCdMask = mask;
            sidekickManager.skillCdText = text;
        }
    }
}