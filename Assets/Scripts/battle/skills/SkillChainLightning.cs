using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using battle;

namespace battle
{
    public class SkillChainLightning : BaseSkillController
    {
        [Header("Chain Lightning Settings")]
        public LineRenderer lightningLine;
        public ParticleSystem hitEffect;
        
        private List<MonsterManager> hitTargets = new List<MonsterManager>();
        private int maxChainCount = 4;
        private float chainRange = 4f;
        private float damageReduction = 0.8f; // Each chain does 80% of previous damage
        
        protected override void Init()
        {
            // Setup line renderer if not assigned
            if (lightningLine == null)
                lightningLine = GetComponent<LineRenderer>();
                
            if (lightningLine == null)
            {
                lightningLine = gameObject.AddComponent<LineRenderer>();
                SetupLineRenderer();
            }
            
            // Setup particle system if not assigned  
            if (hitEffect == null)
                hitEffect = GetComponentInChildren<ParticleSystem>();
                
            StartCoroutine(ExecuteChainLightning());
        }
        
        private void SetupLineRenderer()
        {
            // Use Unlit shader for better visibility
            lightningLine.material = new Material(Shader.Find("Unlit/Color"));
            lightningLine.material.color = new Color(0.3f, 0.8f, 1f, 1f); // Bright electric blue
            lightningLine.startWidth = 0.3f;
            lightningLine.endWidth = 0.15f;
            lightningLine.positionCount = 2;
            lightningLine.useWorldSpace = true;
            lightningLine.sortingOrder = 10; // Render above other sprites
        }
        
        private IEnumerator ExecuteChainLightning()
        {
            // Find initial target
            MonsterManager primaryTarget = GetNearestTarget();
            if (primaryTarget == null)
            {
                yield break;
            }
            
            // Start chain from skill position
            Vector3 currentPosition = transform.position;
            MonsterManager currentTarget = primaryTarget;
            float currentDamageMultiplier = 1f;
            
            // Execute chain sequence
            for (int chainIndex = 0; chainIndex <= maxChainCount && currentTarget != null; chainIndex++)
            {
                // Add to hit targets to prevent re-hitting
                hitTargets.Add(currentTarget);
                
                // Draw lightning arc
                DrawLightningArc(currentPosition, currentTarget.transform.position);
                
                // Deal damage with current multiplier
                AttackTarget(currentTarget, currentDamageMultiplier);
                
                // Play hit effect
                PlayHitEffect(currentTarget.transform.position);
                
                // Lightning chaining delay
                yield return new WaitForSeconds(0.1f);
                
                // Find next target for chain
                Vector3 nextPosition = currentTarget.transform.position;
                MonsterManager nextTarget = GetNearestChainTarget(nextPosition);
                
                // Update for next iteration
                currentPosition = nextPosition;
                currentTarget = nextTarget;
                currentDamageMultiplier *= damageReduction;
                
                if (nextTarget == null)
                {
                    break;
                }
            }
            
            // Brief pause before cleanup
            yield return new WaitForSeconds(0.3f);
            ClearLightningEffect();
            
            // Destroy skill object
            Destroy(gameObject, 0.5f);
        }
        
        private MonsterManager GetNearestTarget()
        {
            var monsters = FindObjectsOfType<MonsterManager>();
            MonsterManager nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var monster in monsters)
            {
                if (monster != null && !monster.isDead)
                {
                    float distance = Vector3.Distance(transform.position, monster.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = monster;
                    }
                }
            }
            
            return nearest;
        }
        
        private MonsterManager GetNearestChainTarget(Vector3 fromPosition)
        {
            var monsters = FindObjectsOfType<MonsterManager>();
            MonsterManager nearest = null;
            float nearestDistance = chainRange;
            
            foreach (var monster in monsters)
            {
                if (monster != null && !monster.isDead && !hitTargets.Contains(monster))
                {
                    float distance = Vector3.Distance(fromPosition, monster.transform.position);
                    if (distance <= chainRange && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = monster;
                    }
                }
            }
            
            return nearest;
        }
        
        private void AttackTarget(MonsterManager target, float damageMultiplier)
        {
            if (target == null || target.isDead) return;
            
            // Calculate damage with chain reduction
            float finalDamageRatio = Skill.DamageRatio * damageMultiplier;
            
            // Apply damage
            var result = Living.Attack(Skill.DamageType, target.Monster, finalDamageRatio);
            target.BeHarmed(result);
            target.CheckDead();
            
            // Apply any buffs from skill
            var buffs = Skill.ActiveCharacter.FindAll(character => character.Contains("ADD_BUFF_"));
            buffs.ForEach(buff => AppendBuff(target.Monster, buff));
        }
        
        private void DrawLightningArc(Vector3 start, Vector3 end)
        {
            // Create jagged lightning path with multiple segments
            List<Vector3> lightningPath = CreateJaggedLightningPath(start, end, 6);
            
            // Create multiple overlapping lightning bolts for dramatic effect
            for (int boltIndex = 0; boltIndex < 2; boltIndex++)
            {
                for (int i = 0; i < lightningPath.Count - 1; i++)
                {
                    Vector3 segmentStart = lightningPath[i];
                    Vector3 segmentEnd = lightningPath[i + 1];
                    
                    // Add slight random variation to each bolt
                    if (boltIndex > 0)
                    {
                        segmentStart += new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), 0);
                        segmentEnd += new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), 0);
                    }
                    
                    CreateLightningSegment(segmentStart, segmentEnd, boltIndex);
                }
                
                // Add some branching effects
                if (boltIndex == 0 && lightningPath.Count > 3)
                {
                    Vector3 branchStart = lightningPath[lightningPath.Count / 2];
                    Vector3 branchEnd = branchStart + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                    CreateLightningSegment(branchStart, branchEnd, 2, true);
                }
            }
        }
        
        private List<Vector3> CreateJaggedLightningPath(Vector3 start, Vector3 end, int segments)
        {
            List<Vector3> path = new List<Vector3> { start };
            
            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector3 basePoint = Vector3.Lerp(start, end, t);
                
                // Add random zigzag effect
                Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.forward);
                float zigzag = Random.Range(-0.8f, 0.8f);
                Vector3 jaggedPoint = basePoint + perpendicular * zigzag;
                
                // Add some random forward/backward variation too
                Vector3 forward = (end - start).normalized;
                jaggedPoint += forward * Random.Range(-0.3f, 0.3f);
                
                path.Add(jaggedPoint);
            }
            
            path.Add(end);
            return path;
        }
        
        private void CreateLightningSegment(Vector3 segmentStart, Vector3 segmentEnd, int boltIndex, bool isBranch = false)
        {
            GameObject lightningSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lightningSegment.name = $"LightningSegment_{boltIndex}";
            
            // Position and scale
            Vector3 midPoint = (segmentStart + segmentEnd) / 2f;
            lightningSegment.transform.position = midPoint;
            
            float distance = Vector3.Distance(segmentStart, segmentEnd);
            float thickness = isBranch ? Random.Range(0.04f, 0.12f) : Random.Range(0.08f, 0.24f); // 2x thicker
            lightningSegment.transform.localScale = new Vector3(thickness, distance / 2f, thickness);
            
            // Rotation
            Vector3 direction = (segmentEnd - segmentStart).normalized;
            lightningSegment.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            
            // Electric colors with intensity variation
            var renderer = lightningSegment.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            
            Color[] electricColors = { 
                Color.white, 
                new Color(0.8f, 0.95f, 1f), 
                new Color(0.6f, 0.9f, 1f), 
                Color.cyan 
            };
            
            Color baseColor = electricColors[Random.Range(0, electricColors.Length)];
            if (isBranch) baseColor *= 0.7f; // Branches are dimmer
            renderer.material.color = baseColor;
            
            // Remove collider
            Destroy(lightningSegment.GetComponent<Collider>());
            
            // Quick flash timing
            float destroyTime = Random.Range(0.06f, 0.18f);
            Destroy(lightningSegment, destroyTime);
        }
        
        private void PlayHitEffect(Vector3 position)
        {
            if (hitEffect != null)
            {
                hitEffect.transform.position = position;
                hitEffect.Play();
            }
        }
        
        private void ClearLightningEffect()
        {
            if (lightningLine != null)
            {
                lightningLine.positionCount = 0;
            }
        }
        
        protected override void WhenAttackAfter()
        {
            // Custom behavior after each attack in chain
        }
        
        private void OnDrawGizmosSelected()
        {
            // Debug visualization
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chainRange);
            
            // Show hit targets
            Gizmos.color = Color.red;
            foreach (var target in hitTargets)
            {
                if (target != null)
                    Gizmos.DrawWireSphere(target.transform.position, 0.5f);
            }
        }
    }
}