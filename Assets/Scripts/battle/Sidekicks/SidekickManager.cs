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
                Debug.Log($"[SidekickManager] Disabled Animator for manual sprite animation");
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
            
            Debug.Log($"[SidekickManager] Initializing sidekick: {sidekick.Name} (ID: {sidekick.Id})");
            
            // Don't use Animator - it's disabled. Animation will be handled manually like HeroManager
            isActive = true;
            
            // TODO: Implement manual sprite animation like HeroManager.AnimateHeroBack()
            Debug.Log($"[SidekickManager] Sidekick {sidekick.Name} initialized - using manual sprite animation");
            
            // Load sidekick sprite with validation
            // Map API IDs to actual sprite file IDs (API sends 5,7,9,4 but files are B_01_, B_02_, etc.)
            int spriteId = GetSpriteIdForSidekick(sidekick.Name);
            string spritePath = Path.GetPath(Path.SidekickBackSprite, sidekick.Name, spriteId.ToString("00"));
            Debug.Log($"[SidekickManager] Loading sidekick sprite for {sidekick.Name} (API ID: {sidekick.Id}, Sprite ID: {spriteId}) from path: {spritePath}");
            
            // If sidekickBack is null, try to find the SpriteRenderer component
            if (sidekickBack == null)
            {
                sidekickBack = GetComponentInChildren<SpriteRenderer>();
                Debug.Log($"[SidekickManager] sidekickBack was null, found SpriteRenderer: {sidekickBack != null}");
            }
            
            // DEBUG: Check all SpriteRenderer components
            var allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            Debug.Log($"[SidekickManager] Found {allSpriteRenderers.Length} SpriteRenderer components:");
            for (int i = 0; i < allSpriteRenderers.Length; i++)
            {
                Debug.Log($"[SidekickManager] SpriteRenderer[{i}]: {allSpriteRenderers[i].gameObject.name}, current sprite: {allSpriteRenderers[i].sprite?.name ?? "null"}");
            }
            
            Sprite sidekickSprite = Resources.Load<Sprite>(spritePath);
            Debug.Log($"[SidekickManager] sidekickBack null check: {sidekickBack == null}, sprite null check: {sidekickSprite == null}");
            
            // DEBUG: Test direct path that we know works
            if (sidekickSprite == null)
            {
                string testPath = "Sidekicks/Back/B_05_Lyanna/B_05_1";
                Sprite testSprite = Resources.Load<Sprite>(testPath);
                Debug.Log($"[SidekickManager] DIRECT TEST: {testPath} -> {testSprite?.name ?? "NULL"}");
            }
            Debug.Log($"[SidekickManager] CRITICAL: Sprite loading result - Path: {spritePath}, Loaded sprite: {sidekickSprite?.name ?? "NULL - FILE NOT FOUND!"}");
            
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
    }
}