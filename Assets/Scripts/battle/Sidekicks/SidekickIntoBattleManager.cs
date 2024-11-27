using System;
using System.Collections.Generic;
using entity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using utils;
using Image = UnityEngine.UI.Image;

namespace battle
{
    public class SidekickIntoBattleManager : MonoBehaviour
    {
        public static SidekickIntoBattleManager Instance { get; private set; }
        public GameObject parent;
        public GameObject skillCdPrefab;
        public List<Transform> SidekickPositions;
        public List<GameObject> SkillsCds = new();
        
        public List<GameObject> Sidekicks = new();

        private void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            IntoBattle(0, new Sidekick
            {
                Name = "sidekick1",
                Hp = 100,
                Atk = 40,
                Skill = new Skill
                {
                    Name = "Skill2",
                    Icon = "skill_icon_117",
                    Duration = 1.9f,
                    Cd = 5,
                    IsDurationDamage = false,
                    MultiplyingPower = 1,
                    DamageType = DamageType.Light,
                    AttackFrequency = 1,
                    AdditionalBuff = new BurnBuff(5, 1, 15)
                }
            });
            
            // IntoBattle(1, new Sidekick
            // {
            //     Name = "sidekick2",
            //     Hp = 100,
            //     Atk = 5,
            //     Skill = new Skill
            //     {
            //         Name = "Skill2",
            //         Icon = "skill_icon_57",
            //         Duration = 3f,
            //         Cd = 10,
            //         IsDurationDamage = false,
            //         MultiplyingPower = 1,
            //         DamageType = DamageType.Ice,
            //         AttackFrequency = 0.3f,
            //         AdditionalBuff = new FrozenBuff( 3)
            //     }
            // });
            
            IntoBattle(2, new Sidekick
            {
                Name = "sidekick3",
                Hp = 100,
                Atk = 40,
                Skill = new Skill
                {
                    Name = "Skill3",
                    Icon = "skill_icon_27",
                    Duration = 1f,
                    Cd = 5,
                    IsDurationDamage = false,
                    MultiplyingPower = 1,
                    DamageType = DamageType.Ice,
                    AttackFrequency = 1,
                    AdditionalBuff = new FrozenBuff(5)
                }
            });
            
        }


        public void IntoBattle(int index, Sidekick sidekick)
        {
            var prefab = LoadPrefab.Load("sidekicks/" + sidekick.Name);
            if (prefab == null)
            {
                Debug.LogError($"Sidekick prefab for {sidekick.Name} not found.");
                return;
            }

            var sidekickObj = Instantiate(prefab, SidekickPositions[index].position, Quaternion.identity,
                parent.transform);
            sidekickObj.transform.Rotate(-90f, 0, 0);
            
            var sidekickManager = sidekickObj.GetComponent<SidekickManager>();
            if (sidekickManager == null)
            {
                Debug.LogError($"SidekickManager component not found in {sidekick.Name} prefab.");
                return;
            }

            Sidekicks.Add(sidekickObj);
            SidekickPositions[index].gameObject.SetActive(false);
            var skillCd = Instantiate(skillCdPrefab, SkillsCds[index].transform, true);
            var rectTransform = skillCd.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero; // 如果你需要在锚点位置
            rectTransform.offsetMin = Vector2.zero; // 确保左下角偏移是0
            rectTransform.offsetMax = Vector2.zero; // 确保右上角偏移是0
            
            
            rectTransform.localScale = new Vector3(1, 1, 1);
            var image = skillCd.GetComponent<Image>();
            Sprite[] sprites = Resources.LoadAll<Sprite>("skills/ICON/skill_icon");
            Sprite icon = null;
            if (sprites != null && sprites.Length > 0)
            {
                foreach (var sprite in sprites)
                {
                    if (sprite.name == sidekick.Skill.Icon)
                    {
                        icon = sprite;
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError("skills/ICON/skill_icon/" + sidekick.Skill.Icon + " not found.");
            }
            if(icon == null) return;
            image.sprite = icon;
            GameObject mask = skillCd.transform.Find("skillCD_Mask").gameObject;
            Debug.Log(mask);
            image = mask.GetComponent<Image>();
            image.sprite = icon;
            GameObject text = skillCd.transform.Find("skillCD_Text").gameObject;
            // text.GetComponent<Text>().text = sidekick.Skill.Cd.ToString() + "S";
            sidekickManager.Sidekick = sidekick;
            sidekickManager.skillCd = skillCd;
            sidekickManager.skillCdMask = mask;
            sidekickManager.skillCdText = text;
            sidekickManager.isActive = true;
        }
    }
}