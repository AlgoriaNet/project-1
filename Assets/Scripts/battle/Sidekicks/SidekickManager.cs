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
            // Disable animator like HeroManager does - use manual sprite animation instead
            if (_animator != null)
            {
                _animator.enabled = false;
            }
        }

        protected virtual void Update()
        {
            if (!isActive || _sidekick == null || _sidekick.Skill == null) return;
            UpdateWaitingTime();
            UpdateSkillCooldownUI();
            if (_waitingTime >= _sidekick.Skill.Cd) HandleSkillRelease();
        }

        public void Init(Sidekick sidekick)
        {
            if (sidekick == null)
            {
                Debug.LogError("[SidekickManager] Cannot init with null sidekick!");
                return;
            }
            
            _sidekick = sidekick;
            _isReleasing = false;
            _launchesTimes = 0;
            _waitingTime = 0;
            _waitLaunchesIntervalTime = 0;
            
            // Don't use Animator - it's disabled. Animation will be handled manually like HeroManager
            isActive = true;
            
            // Start manual sprite animation like HeroManager does
            StartCoroutine(AnimateSidekickBack());
            
            // Load sidekick sprite with validation
            // Map API IDs to actual sprite file IDs (API sends 5,7,9,4 but files are B_01_, B_02_, etc.)
            int spriteId = GetSpriteIdForSidekick(sidekick.Name);
            string spritePath = Path.GetPath(Path.SidekickBackSprite, sidekick.Name, spriteId.ToString("00"));
            
            // If sidekickBack is null, try to find the SpriteRenderer component
            if (sidekickBack == null)
            {
                sidekickBack = GetComponentInChildren<SpriteRenderer>();
            }
            
            Sprite sidekickSprite = Resources.Load<Sprite>(spritePath);
            
            if (sidekickSprite != null)
            {
                if (sidekickBack != null)
                {
                    Debug.Log($"[SidekickManager] About to assign sprite {sidekickSprite.name} to sidekickBack.sprite");
                    Debug.Log($"[SidekickManager] sidekickBack reference: {sidekickBack}, enabled: {sidekickBack.enabled}");
                    Debug.Log($"[SidekickManager] sidekickBack.gameObject: {sidekickBack.gameObject.name}");
                    
                    sidekickBack.sprite = sidekickSprite;
                    
                    Debug.Log($"[SidekickManager] After assignment - sidekickBack.sprite is: {sidekickBack.sprite?.name ?? "null"}");
                    
                    // Force assignment verification
                    if (sidekickBack.sprite == null) {
                        Debug.LogError($"[SidekickManager] CRITICAL: Sprite assignment failed! Trying direct assignment...");
                        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                        if (spriteRenderer != null) {
                            spriteRenderer.sprite = sidekickSprite;
                            Debug.Log($"[SidekickManager] Direct assignment result: {spriteRenderer.sprite?.name ?? "null"}");
                        }
                    }
                    
                    Debug.Log($"[SidekickManager] SUCCESS: Loaded sidekick sprite: {sidekickSprite.name} for {sidekick.Name}");
                    
                    // Ensure sidekick doesn't overlay hero position and has proper sorting order
                    EnsureProperPositioning();
                    
                    // TEMPORARY: Make sidekicks bigger for testing visibility
                    transform.localScale = Vector3.one; // Change from 0.25 to 1.0 for testing
                    Debug.Log($"[SidekickManager] TEMP: Set {sidekick.Name} scale to 1.0 for testing");
                }
                else
                {
                    Debug.LogError($"[SidekickManager] ERROR: sidekickBack SpriteRenderer is null! Cannot assign sprite for {sidekick.Name}");
                }
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
            
            // Set sorting layer to match hero's "Default" layer
            sidekickBack.sortingLayerName = "Default";
            sidekickBack.sortingOrder = 200;
            
        }
        
        /// <summary>
        /// Maps sidekick names to their actual sprite file IDs
        /// API IDs actually match the sprite folder names correctly!
        /// </summary>
        private int GetSpriteIdForSidekick(string sidekickName)
        {
            // The API IDs are correct - directories are B_01_Zorath, B_05_Lyanna, B_07_Elenya, etc.
            // Just use the API ID directly
            switch (sidekickName)
            {
                case "Zorath": return 1;
                case "Lyanna": return 5; 
                case "Elenya": return 7;
                case "Liraen": return 9;
                case "Aurelia": return 4;
                default:
                    Debug.LogWarning($"[SidekickManager] Unknown sidekick name: {sidekickName}, using ID 1 as fallback");
                    return 1;
            }
        }
        
        /// <summary>
        /// Manual sprite animation like HeroManager.AnimateHeroBack()
        /// Cycles between sprite 1 and sprite 2 for each sidekick
        /// </summary>
        private IEnumerator AnimateSidekickBack()
        {
            if (sidekickBack == null || _sidekick == null)
            {
                yield break;
            }
            
            int spriteId = GetSpriteIdForSidekick(_sidekick.Name);
            string[] spriteNames = { $"B_{spriteId:00}_1", $"B_{spriteId:00}_2" };
            int currentFrame = 0;
            
            
            while (true)
            {
                string currentSpriteName = spriteNames[currentFrame];
                string spritePath = $"Sidekicks/Back/B_{spriteId:00}_{_sidekick.Name}/{currentSpriteName}";
                
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                if (sprite != null && sidekickBack != null)
                {
                    sidekickBack.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"[SidekickManager] Failed to load animation frame: {spritePath}");
                }
                
                // Toggle between frame 0 and 1
                currentFrame = 1 - currentFrame;
                
                yield return new WaitForSeconds(0.2f); // 4x speed - was 0.8f
            }
        }
    }
}