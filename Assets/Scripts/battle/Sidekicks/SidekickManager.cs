using System;
using System.Collections;
using entity;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace battle
{
    public class SidekickManager : MonoBehaviour
    {
        public Sidekick Sidekick;
        public float waitingTime;
        public float releaseSkillTime;
        public bool isActive;
        public bool isReleasing;

        [Header("Manager Controller")] 
        protected SkeletonAnimation _animationController;

        [Header("Objects Controller")]
        public GameObject skillParent;
        public GameObject skillPrefab;
        public Transform ReleaseSkillPosition;
        
        [Header("ICON Controller")]
        public GameObject skillCd;
        public GameObject skillCdText;
        public GameObject skillCdMask;
        
        [Header("playanimationname")] 
        public string idle_name = null;
        public string dead_name = null;
        public string attack_name = null;
        public string wark_name = null;
        
        protected void Awake()
        {
            _animationController = GetComponentInChildren<SkeletonAnimation>();
            if (_animationController != null)
            {
                releaseSkillTime = _animationController.AnimationState.Data.SkeletonData.FindAnimation(attack_name).Duration;
                _animationController.AnimationState.SetAnimation(0, idle_name, true);
            }
                
        }
        
        protected virtual void Update()
        {
            if(!isActive)  return;
            UpdateWaitingTime();
            UpdateSkillCooldownUI();
            if (waitingTime >= Sidekick.Skill.Cd) HandleSkillRelease();
            
        }

        protected virtual IEnumerator ReleaseSkill()
        {
            string skillName = Sidekick.Skill.Name;
            if (skillPrefab == null)
            {
                Debug.LogError($"Skill prefab for {skillName} not found.");
                yield return null;
            }
            if(_animationController != null)
                _animationController.AnimationState.SetAnimation(0, attack_name, false);
            new WaitForSeconds(releaseSkillTime);
            isReleasing = true;
            GameObject skillObject = Instantiate(skillPrefab);
            skillObject.transform.position = this.getSkillPosition();
            var skillWrapperManager = skillObject.GetComponent<SkillWrapperManager>();
            if (skillWrapperManager == null)
                Debug.LogError("SkillWrapperManager component not found on skill prefab.");
            else
            {
                var skillManager = skillWrapperManager.skill.GetComponent<SkillManager>();
                if (skillManager == null)
                    Debug.LogError("SkillManager component not found on skill prefab.");
                else
                {
                    skillManager.Sidekick = Sidekick;
                    Destroy(skillObject, Sidekick.Skill.Duration);
                }
            }
            yield return new WaitForSeconds(Sidekick.Skill.Duration);
            OnSkillReleased();
        }


        public virtual Vector3 getSkillPosition()
        {
            return MonsterGridManager.Instance.CentralPoint();
        }
        
        protected void UpdateWaitingTime()
        {
            waitingTime += Time.deltaTime;
        }
        
        protected void UpdateSkillCooldownUI()
        {
            if (Sidekick == null) return;

            UpdateSkillCooldownText();
            UpdateSkillCooldownMask();
        }

        private void UpdateSkillCooldownText()
        {
            if (skillCdText == null) return;

            var component = skillCdText.GetComponent<TMP_Text>();
            if (component == null) return;

            double remainingTime = Sidekick.Skill.Cd - waitingTime;
            component.text = remainingTime > 0 ? Math.Round(remainingTime, 1).ToString() + "S" : "";
        }

        private void UpdateSkillCooldownMask()
        {
            if (skillCdMask == null) return;

            var imageComponent = skillCdMask.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.fillAmount = 1 - waitingTime / Sidekick.Skill.Cd;
            }
        }
        
        protected virtual void HandleSkillRelease()
        {
            if (isReleasing) return;
            StartCoroutine(ReleaseSkill());
        }
        
        protected virtual void OnSkillReleased()
        {
            waitingTime = 0;
            isReleasing = false;
            if (_animationController != null)
                _animationController.AnimationState.SetAnimation(0, idle_name, true);
        }
    }
}