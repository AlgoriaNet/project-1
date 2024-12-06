using System;
using System.Collections;
using entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using utils;

namespace battle
{
    public class SidekickManager : MonoBehaviour
    {
        [SerializeField] private Transform releaseSkillPosition;
        [SerializeField] private SpriteRenderer sidekickBack;
        [Header("Objects Controller")] public GameObject skillParent;

        [HideInInspector] public bool isActive;
        [HideInInspector] public GameObject skillCdText;
        [HideInInspector] public GameObject skillCdMask;

        private Animator _animator;
        private GameObject _skillPrefab;
        private Sidekick _sidekick;
        private bool _isReleasing;
        private int _launchesTimes;
        private float _waitingTime;
        private float _waitLaunchesIntervalTime;
        private float _releaseSkillTime;
        
        protected void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _animator.Play($"_{_sidekick.Name}_Attack");
            isActive = true;
        }

        protected virtual void Update()
        {
            if (!isActive) return;
            UpdateWaitingTime();
            UpdateSkillCooldownUI();
            if (_waitingTime >= _sidekick.Skill.Cd) HandleSkillRelease();
        }

        public void Init(Sidekick sidekick)
        {
            _sidekick = sidekick;
            _isReleasing = false;
            _launchesTimes = 0;
            _waitingTime = 0;
            _waitLaunchesIntervalTime = 0;

            var skillPrefab = Resources.Load<GameObject>(Path.GetPath(Path.SkillPrefab, sidekick.Skill.Name));
            Debug.Log($"SKill Path: {Path.GetPath(Path.SkillPrefab, sidekick.Skill.Name)}");
            Debug.Log($"skill prefab: {skillPrefab}");
            if(skillPrefab) _skillPrefab = skillPrefab;
            sidekickBack.sprite = Resources.Load<Sprite>(Path.GetPath(Path.SidekickBackSprite, sidekick.Name));
        }

        private IEnumerator ReleaseSkill()
        {
            _isReleasing = true;
            yield return null;
            for (int i = 0; i < _sidekick.Skill.ReleaseCount; i++)
            {
                GameObject skillObject = Instantiate(_skillPrefab);
                var skillManager = skillObject.GetComponent<SkillSetting>();
                skillManager.Living = _sidekick;
                skillManager.targetIndex = i;
                if (_sidekick.Skill.IsLivingPositionRelease)
                {
                    skillObject.transform.position = releaseSkillPosition.position;
                    var targetPosition = BattleGridManager.Instance.GetTargetPosition(_sidekick.Skill.TargetType, i);
                    if (_sidekick.Skill.TargetType == SkillTargetType.LatestNearby)
                        skillManager.targetDirection = Utils.AngleOffsetDirection(
                            targetPosition - releaseSkillPosition.position,
                            _sidekick.Skill.MaxAngle, _sidekick.Skill.ReleaseCount, i);
                    else
                        skillManager.targetDirection = (targetPosition - releaseSkillPosition.position).normalized;
                }
                else
                    skillObject.transform.position =
                        BattleGridManager.Instance.GetTargetPosition(_sidekick.Skill.TargetType, i);
            }
            OnSkillReleased();
        }

        private void UpdateWaitingTime()
        {
            _waitingTime += Time.deltaTime;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void UpdateSkillCooldownUI()
        {
            if (_sidekick == null) return;
            UpdateSkillCooldownText();
            UpdateSkillCooldownMask();
        }

        private void UpdateSkillCooldownText()
        {
            if (skillCdText == null) return;

            var component = skillCdText.GetComponent<TMP_Text>();
            double remainingTime = _sidekick.Skill.Cd - _waitingTime;
            component.text = remainingTime > 0 && _waitingTime > 0 ? Math.Round(remainingTime, 1) + "S" : "";
        }

        private void UpdateSkillCooldownMask()
        {
            if (skillCdMask == null) return;

            var imageComponent = skillCdMask.GetComponent<Image>();
            var percentage = _waitingTime / _sidekick.Skill.Cd;
            if (_waitingTime < 0) percentage = 1;
            if (imageComponent != null) imageComponent.fillAmount = 1 - percentage;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void HandleSkillRelease()
        {
            if (_isReleasing || _skillPrefab == null) return;
            _waitLaunchesIntervalTime += Time.deltaTime;
            if (_launchesTimes > 0 && _waitLaunchesIntervalTime < _sidekick.Skill.LaunchesInterval) return;
            _launchesTimes++;
            StartCoroutine(ReleaseSkill());
        }

        private void OnSkillReleased()
        {
            _isReleasing = false;
            if (_launchesTimes == _sidekick.Skill.LaunchesCount)
            {
                _waitingTime = _sidekick.Skill.IsCdRestByReleased ? 0 : -_sidekick.Skill.Duration;
                _launchesTimes = 0;
            }

            _waitLaunchesIntervalTime = 0;
        }
    }
}