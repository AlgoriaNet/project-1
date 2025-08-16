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
                // Calculate proper position to avoid hero overlap
                Vector3 targetPosition = sidekickPositions[index].position;
                
                // Adjust sidekick positioning: move up and spread out slightly
                targetPosition.y += 0.5f; // Move up so feet aren't off screen
                
                // Spread them out more horizontally based on index
                float spreadOffset = (index - 1.5f) * 0.4f; // -0.45, -0.15, +0.15, +0.45
                targetPosition.x += spreadOffset;
                
                // Special positioning for flame sidekicks to prevent hero visual confusion
                if (sidekick.Name.ToLower().Contains("flame"))
                {
                    // Place flame sidekicks further to the right
                    targetPosition.x += 2.0f;
                    Debug.LogWarning($"[SidekickIntoBattle] Applied special positioning for flame sidekick {sidekick.Name}");
                }
                
                var sidekickObj = Instantiate(baseSidekickPrefab,
                    targetPosition,
                    Quaternion.identity,
                    parent.transform);
                var sidekickManager = sidekickObj.GetComponent<SidekickManager>();
                sidekickManager.Init(sidekick);
                // Don't disable position markers - they should stay active for proper sidekick visibility
                SetSkillCd(index, sidekick, sidekickManager);
                
                Debug.Log($"[SidekickIntoBattle] Successfully spawned {sidekick.Name} at position {targetPosition}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SidekickIntoBattle] Failed to spawn sidekick {sidekick.Name}: {e}");
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
            
            // Check if sidekick has a valid skill before trying to load icon
            if (sidekick.Skill == null)
            {
                Debug.LogWarning($"[SidekickIntoBattleManager] Sidekick {sidekick.Name} has null Skill - cannot set skill CD UI");
                return;
            }
            
            Sprite icon = sidekick.Skill.LoadIconSprite();
            if (icon == null) 
            {
                Debug.LogWarning($"[SidekickIntoBattleManager] Could not load skill icon for {sidekick.Name}");
                return;
            }
            
            image.sprite = icon;
            GameObject mask = skillCd.transform.Find("skillCD_Mask").gameObject;
            mask.GetComponent<Image>().sprite = icon;
            GameObject text = skillCd.transform.Find("skillCD_Text").gameObject;
            sidekickManager.skillCdMask = mask;
            sidekickManager.skillCdText = text;
        }
    }
}