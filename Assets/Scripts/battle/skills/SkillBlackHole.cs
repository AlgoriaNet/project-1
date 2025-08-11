using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using battle;

namespace battle
{
    public class SkillBlackHole : BaseSkillController
    {
        [Header("Black Hole Settings")]
        public ParticleSystem gravitationalEffects;
        public LineRenderer[] gravitationalTrails;
        
        private List<MonsterManager> affectedEnemies = new List<MonsterManager>();
        private Vector3 blackHoleCenter;
        
        [Header("Black Hole Size & Power")]
        private float blackHoleScale = 1.5f;            // Black hole visual size  
        private float blackHoleDuration = 4.5f;         // How long black hole lasts
        private float pullRange = 3f;                   // Attack radius (2x visual scale)
        private float pullStrength = 2f;                // Pull force (scaled proportionally)
        private float baseDamageInterval = 0.3f;        // Damage every 0.3 seconds
        private float maxDamageMultiplier = 3f;         // 3x damage at center
        
        protected override void Init()
        {
            // Calculate proportional pull range from visual size
            pullRange = blackHoleScale * 2f; // 2x visual size for attack radius
            
            // Set black hole position - either at target or skill spawn point
            if (BattleGridManager.Instance != null)
            {
                blackHoleCenter = BattleGridManager.Instance.GetTargetPosition(Skill.SkillTargetType);
            }
            else
            {
                blackHoleCenter = transform.position;
            }
            
            transform.position = blackHoleCenter;
            
            StartCoroutine(ExecuteBlackHole());
        }
        
        private IEnumerator ExecuteBlackHole()
        {
            // Create visual black hole
            CreateBlackHoleVisuals();
            
            float elapsed = 0f;
            float nextDamageTime = 0f;
            
            while (elapsed < blackHoleDuration)
            {
                // Find and pull enemies
                UpdateAffectedEnemies();
                ApplyGravitationalPull();
                
                // Apply damage at intervals
                if (elapsed >= nextDamageTime)
                {
                    ApplyBlackHoleDamage();
                    nextDamageTime = elapsed + baseDamageInterval;
                }
                
                // Update visual effects
                UpdateGravitationalTrails();
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Clean up effects  
            ClearBlackHoleEffects();
            
            // Destroy skill object immediately after cleanup
            Destroy(gameObject, 0.5f);
        }
        
        private void CreateBlackHoleVisuals()
        {
            // Create main black hole disk
            CreateBlackHoleDisk();
            
            // Create gravitational distortion effects
            CreateGravitationalParticles();
        }
        
        private void CreateBlackHoleDisk()
        {
            // Create black hole disk
            GameObject blackHoleDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            blackHoleDisk.name = "BlackHoleDisk";
            blackHoleDisk.transform.position = blackHoleCenter;
            blackHoleDisk.transform.localScale = new Vector3(blackHoleScale, 0.05f, blackHoleScale); // Flat disk
            blackHoleDisk.transform.Rotate(90, 0, 0); // Rotate to be horizontal
            
            var renderer = blackHoleDisk.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.color = new Color(0.02f, 0.02f, 0.03f, 0.8f); // Deep black

            Destroy(blackHoleDisk.GetComponent<Collider>());
            
            // IMPORTANT: Destroy disk after exact duration
            Destroy(blackHoleDisk, blackHoleDuration + 0.1f);
            
            // Animate disk pulsing
            StartCoroutine(AnimateBlackHoleDisk(blackHoleDisk));
        }
        
        private void CreateGravitationalParticles()
        {
            // Create multiple particle streams spiraling into black hole
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f; // 8 streams around the black hole
                Vector3 streamStart = blackHoleCenter + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * pullRange,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * pullRange,
                    0);
                
                CreateGravitationalStream(streamStart, i);
            }
        }
        
        private void CreateGravitationalStream(Vector3 startPos, int streamIndex)
        {
            // Create small particles that spiral toward center
            StartCoroutine(AnimateGravitationalStream(startPos, streamIndex));
        }
        
        private IEnumerator AnimateGravitationalStream(Vector3 startPos, int streamIndex)
        {
            float streamDuration = blackHoleDuration;
            float elapsed = 0f;
            
            while (elapsed < streamDuration)
            {
                // Create particle every 0.1 seconds
                if (elapsed % 0.1f < Time.deltaTime)
                {
                    GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    particle.name = $"GravityParticle_{streamIndex}";
                    particle.transform.position = startPos + Random.insideUnitSphere * 0.5f;
                    particle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);
                    
                    var renderer = particle.GetComponent<Renderer>();
                    renderer.material = new Material(Shader.Find("Unlit/Color"));
                    renderer.material.color = new Color(0.4f, 0.4f, 0.5f, 0.8f); // Dark grey particles
                    
                    Destroy(particle.GetComponent<Collider>());
                    
                    // Animate particle toward black hole
                    StartCoroutine(AnimateParticleToCenter(particle));
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        private IEnumerator AnimateParticleToCenter(GameObject particle)
        {
            Vector3 startPos = particle.transform.position;
            float travelTime = Random.Range(0.8f, 1.5f);
            float elapsed = 0f;
            
            while (elapsed < travelTime && particle != null)
            {
                float t = elapsed / travelTime;
                // Spiral motion toward center
                float spiralRadius = Mathf.Lerp(Vector3.Distance(startPos, blackHoleCenter), 0.1f, t);
                float angle = elapsed * 360f; // Full rotation per second
                
                Vector3 spiralPos = blackHoleCenter + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * spiralRadius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * spiralRadius,
                    0);
                
                particle.transform.position = spiralPos;
                
                // Fade out as it approaches center
                var renderer = particle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = Mathf.Lerp(0.8f, 0f, t);
                    renderer.material.color = color;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        
        private IEnumerator AnimateBlackHoleDisk(GameObject blackHoleDisk)
        {
            float elapsed = 0f;
            Vector3 originalScale = blackHoleDisk.transform.localScale;
            
            while (elapsed < blackHoleDuration && blackHoleDisk != null)
            {
                // Pulsing effect
                float pulse = 1f + Mathf.Sin(elapsed * 3f) * 0.3f;
                blackHoleDisk.transform.localScale = new Vector3(originalScale.x * pulse, originalScale.y, originalScale.z * pulse);
                
                // Color intensity variation
                var renderer = blackHoleDisk.GetComponent<Renderer>();
                if (renderer != null)
                {
                    float intensity = 0.6f + Mathf.Sin(elapsed * 2f) * 0.2f;
                    renderer.material.color = new Color(0.02f, 0.02f, 0.03f, intensity);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        private void UpdateAffectedEnemies()
        {
            affectedEnemies.Clear();
            var allMonsters = FindObjectsOfType<MonsterManager>();
            
            foreach (var monster in allMonsters)
            {
                if (monster != null && !monster.isDead)
                {
                    float distance = Vector3.Distance(monster.transform.position, blackHoleCenter);
                    if (distance <= pullRange)
                    {
                        affectedEnemies.Add(monster);
                    }
                }
            }
        }
        
        private void ApplyGravitationalPull()
        {
            foreach (var enemy in affectedEnemies)
            {
                if (enemy != null && !enemy.isDead)
                {
                    Vector3 pullDirection = (blackHoleCenter - enemy.transform.position).normalized;
                    float distance = Vector3.Distance(enemy.transform.position, blackHoleCenter);
                    
                    // Stronger pull when closer (inverse square law-ish)
                    float pullForce = pullStrength * (1f / (distance + 0.5f));
                    
                    // Apply pull (move enemy toward black hole)
                    Vector3 newPosition = enemy.transform.position + pullDirection * pullForce * Time.deltaTime;
                    
                    // Don't let enemies go past the center
                    if (Vector3.Distance(newPosition, blackHoleCenter) < 0.2f)
                    {
                        newPosition = blackHoleCenter + (enemy.transform.position - blackHoleCenter).normalized * 0.2f;
                    }
                    
                    enemy.transform.position = newPosition;
                }
            }
        }
        
        private void ApplyBlackHoleDamage()
        {
            foreach (var enemy in affectedEnemies)
            {
                if (enemy != null && !enemy.isDead)
                {
                    float distance = Vector3.Distance(enemy.transform.position, blackHoleCenter);
                    
                    // Damage increases dramatically as enemies get closer
                    float distanceRatio = 1f - (distance / pullRange); // 0 at edge, 1 at center
                    float damageMultiplier = 1f + (distanceRatio * maxDamageMultiplier);
                    
                    // Apply damage
                    float finalDamageRatio = Skill.DamageRatio * damageMultiplier * 0.3f; // 0.3f since damage is applied frequently
                    var result = Living.Attack(Skill.DamageType, enemy.Monster, finalDamageRatio);
                    enemy.BeHarmed(result);
                    enemy.CheckDead();
                    
                    // Apply any buffs from skill
                    var buffs = Skill.ActiveCharacter.FindAll(character => character.Contains("ADD_BUFF_"));
                    buffs.ForEach(buff => AppendBuff(enemy.Monster, buff));
                }
            }
        }
        
        private void UpdateGravitationalTrails()
        {
            // Visual trails showing matter being pulled in (handled by particle animations)
        }
        
        private void ClearBlackHoleEffects()
        {
            // Clean up any remaining visual effects more aggressively
            var blackHoles = GameObject.FindGameObjectsWithTag("Untagged").Where(obj => 
                obj.name.Contains("BlackHole") || 
                obj.name.Contains("GravityParticle")).ToArray();
            
            foreach (var obj in blackHoles)
            {
                if (obj != null && obj != gameObject) // Don't destroy self
                {
                    Destroy(obj, 0.1f); // Small delay to ensure cleanup
                }
            }
            
            // Also stop any remaining coroutines
            StopAllCoroutines();
        }
        
        protected override void WhenAttackAfter()
        {
            // Custom behavior after black hole effects
        }
        
        private void OnDrawGizmosSelected()
        {
            // Debug visualization
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(blackHoleCenter, pullRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(blackHoleCenter, 0.2f); // Minimum distance
            
            // Show affected enemies
            Gizmos.color = Color.yellow;
            foreach (var enemy in affectedEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(enemy.transform.position, blackHoleCenter);
                }
            }
        }
    }
}