using System;
using System.Collections;
using model;
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
            
            // Load sidekick sprite with validation
            string spritePath = Path.GetPath(Path.SidekickBackSprite, sidekick.Name);
            Debug.Log($"[SidekickManager] Loading sidekick sprite for {sidekick.Name} from path: {spritePath}");
            
            Sprite sidekickSprite = Resources.Load<Sprite>(spritePath);
            if (sidekickSprite != null)
            {
                sidekickBack.sprite = sidekickSprite;
                Debug.Log($"[SidekickManager] SUCCESS: Loaded sidekick sprite: {sidekickSprite.name} for {sidekick.Name}");
                
                // Ensure sidekick doesn't overlay hero position and has proper sorting order
                EnsureProperPositioning();
            }
            else
            {
                Debug.LogError($"[SidekickManager] FAILED: Could not load sidekick sprite from path: {spritePath}");
            }
        }

        private IEnumerator ReleaseSkill()
        {
            _isReleasing = true;
            yield return null;
            for (int i = 0; i < _sidekick.Skill.ReleaseCount; i++)
            {
                var targetPosition = BattleGridManager.Instance.GetTargetPosition(_sidekick.Skill.SkillTargetType, i);
                var targetDirection = _sidekick.Skill.SkillTargetType == SkillTargetType.LatestNearby
                    ? Utils.AngleOffsetDirection(targetPosition - releaseSkillPosition.position, _sidekick.Skill.MaxAngle, _sidekick.Skill.ReleaseCount, i)
                    : (Vector2)(targetPosition - releaseSkillPosition.position).normalized;
                
                var position = _sidekick.Skill.IsLivingPositionRelease ? releaseSkillPosition.position : targetPosition;
                
                SkillFactory.Create( _sidekick.Skill.Name, _sidekick, _sidekick.Skill, i, targetDirection, position);
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
            if (_isReleasing) return;
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
        
        /// <summary>
        /// Ensure sidekick doesn't interfere with hero visibility and has proper positioning
        /// </summary>
        private void EnsureProperPositioning()
        {
            if (sidekickBack == null) return;
            
            // Set appropriate sorting order - always below hero (hero uses 50)
            const int SIDEKICK_MAX_SORTING_ORDER = 40;
            sidekickBack.sortingOrder = SIDEKICK_MAX_SORTING_ORDER;
            
            // Find hero position to avoid overlap
            var heroManager = FindObjectOfType<HeroManager>();
            if (heroManager != null)
            {
                float distanceToHero = Vector3.Distance(transform.position, heroManager.transform.position);
                
                // If sidekick is too close to hero position, move it away
                if (distanceToHero < 1.5f)
                {
                    Vector3 directionFromHero = (transform.position - heroManager.transform.position).normalized;
                    Vector3 newPosition = heroManager.transform.position + directionFromHero * 2.0f;
                    transform.position = newPosition;
                    
                    Debug.LogWarning($"[SidekickManager] Moved sidekick {_sidekick.Name} away from hero. New position: {newPosition}");
                }
                
                // Special handling for flame-type sidekicks to prevent visual confusion
                if (_sidekick.Name.ToLower().Contains("flame"))
                {
                    // Move flame sidekicks further from hero and lower their priority
                    Vector3 flamePosition = heroManager.transform.position + Vector3.right * 3.0f;
                    transform.position = flamePosition;
                    sidekickBack.sortingOrder = SIDEKICK_MAX_SORTING_ORDER - 10; // Even lower priority
                    
                    Debug.LogWarning($"[SidekickManager] Applied special positioning for flame sidekick {_sidekick.Name} at {flamePosition}");
                }
            }
            
            Debug.Log($"[SidekickManager] Configured {_sidekick.Name} positioning - SortingOrder: {sidekickBack.sortingOrder}, Position: {transform.position}");
        }
    }
}