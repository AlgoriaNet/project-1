using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace battle
{
    public class SimpleBattleController : MonoBehaviour
    {
        public static SimpleBattleController Instance;
        
        [Header("HP System")]
        public int maxHp = 100;
        public int currentHp = 100;
        public Image hpBar;
        public TMP_Text hpText;
        
        [Header("Monster Damage")]
        public int monsterDamage = 10;
        public float damageInterval = 2f;
        
        [Header("Visual Feedback")]
        public Transform fence;
        public SpriteRenderer fenceRenderer;
        public Sprite[] fenceDamageSprites; // 0=intact, 1=damaged, 2=broken
        
        private float damageTimer = 0f;
        private bool gameActive = true;
        
        void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                Debug.Log("[SimpleBattleController] Battle controller initialized");
            }
            else 
            {
                Destroy(this);
            }
        }
        
        void Start()
        {
            currentHp = maxHp;
            UpdateUI();
            
            // Find fence if not assigned
            if (fence == null)
            {
                GameObject fenceObj = GameObject.Find("fence");
                if (fenceObj != null)
                {
                    fence = fenceObj.transform;
                    fenceRenderer = fenceObj.GetComponent<SpriteRenderer>();
                }
            }
            
            Debug.Log("[SimpleBattleController] Battle started - HP: " + currentHp);
        }
        
        void Update()
        {
            if (!gameActive) return;
            
            // Simulate monster attacks
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                TakeDamage(monsterDamage);
                damageTimer = 0f;
            }
            
            // Test key for manual damage
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("[SimpleBattleController] Manual damage test triggered!");
                TakeDamage(monsterDamage);
            }
        }
        
        public void TakeDamage(int damage)
        {
            currentHp -= damage;
            currentHp = Mathf.Max(currentHp, 0);
            
            Debug.Log($"[SimpleBattleController] Took {damage} damage! HP: {currentHp}/{maxHp}");
            
            UpdateUI();
            UpdateFenceVisual();
            
            if (currentHp <= 0)
            {
                GameOver();
            }
        }
        
        void UpdateUI()
        {
            if (hpBar != null)
            {
                hpBar.fillAmount = (float)currentHp / maxHp;
            }
            
            if (hpText != null)
            {
                hpText.text = $"{currentHp}/{maxHp}";
            }
        }
        
        void UpdateFenceVisual()
        {
            if (fenceRenderer == null || fenceDamageSprites == null || fenceDamageSprites.Length < 3) 
                return;
                
            float hpPercent = (float)currentHp / maxHp;
            
            if (hpPercent > 0.66f)
            {
                fenceRenderer.sprite = fenceDamageSprites[0]; // Intact
            }
            else if (hpPercent > 0.33f)
            {
                fenceRenderer.sprite = fenceDamageSprites[1]; // Damaged
            }
            else
            {
                fenceRenderer.sprite = fenceDamageSprites[2]; // Broken
            }
        }
        
        void GameOver()
        {
            gameActive = false;
            Debug.Log("[SimpleBattleController] GAME OVER! HP reached 0");
            
            // Show game over UI or restart
            Time.timeScale = 0;
            
            // You can add game over UI here
        }
        
        // Public method for other scripts to cause damage
        public void MonsterAttack(int damage = -1)
        {
            if (damage == -1) damage = monsterDamage;
            TakeDamage(damage);
        }
    }
}