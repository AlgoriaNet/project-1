using UnityEngine;

namespace battle
{
    public class TestMonster : MonoBehaviour
    {
        public float speed = 2f;
        public int damage = 10;
        public float attackInterval = 1f;
        
        private bool reachedBottom = false;
        private float attackTimer = 0f;
        
        void Start()
        {
            Debug.Log("[TestMonster] Monster spawned at: " + transform.position);
        }
        
        void Update()
        {
            if (!reachedBottom)
            {
                // Move downward
                transform.position -= new Vector3(0, speed * Time.deltaTime, 0);
                
                // Check if reached bottom (fence area)
                if (transform.position.y <= -8f)
                {
                    reachedBottom = true;
                    Debug.Log("[TestMonster] Monster reached the fence! Starting attacks...");
                }
            }
            else
            {
                // Attack the fence
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    AttackFence();
                    attackTimer = 0f;
                }
            }
        }
        
        void AttackFence()
        {
            Debug.Log($"[TestMonster] Monster attacking fence for {damage} damage!");
            
            // Find and damage the battle controller
            if (SimpleBattleController.Instance != null)
            {
                SimpleBattleController.Instance.MonsterAttack(damage);
            }
            else
            {
                Debug.LogError("[TestMonster] No SimpleBattleController found!");
            }
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Finish") || other.name.Contains("FinishLine"))
            {
                reachedBottom = true;
                Debug.Log("[TestMonster] Monster triggered finish line collider!");
            }
        }
    }
}