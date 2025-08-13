using UnityEngine;

namespace battle
{
    public class FenceDestructionManager : MonoBehaviour
    {
        public static FenceDestructionManager Instance;
        
        [Header("Fence GameObjects - Assign these in Inspector")]
        public GameObject Wood_0, Wood_1, Wood_2, Wood_3;
        public GameObject Steel_0, Steel_1, Steel_2, Steel_3;
        
        [Header("HP System")]
        public int maxWoodHp = 300;  // Wood is weaker
        public int maxSteelHp = 500; // Steel is stronger
        public int currentWoodHp;
        public int currentSteelHp;
        
        public enum FenceState
        {
            WoodGood = 0,      // Wood_0, Steel_0 active
            WoodDamage1 = 1,   // Wood_1, Steel_0 active
            WoodDamage2 = 2,   // Wood_2, Steel_0 active  
            WoodDamage3 = 3,   // Wood_3, Steel_0 active
            SteelGood = 4,     // Steel_0 active only (wood destroyed)
            SteelDamage1 = 5,  // Steel_1 active only
            SteelDamage2 = 6,  // Steel_2 active only
            SteelDamage3 = 7,  // Steel_3 active only
            Failure = 8        // All inactive
        }
        
        public FenceState currentState = FenceState.WoodGood;
        
        void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                Debug.Log("[FenceDestruction] FenceDestructionManager initialized");
            }
            else 
            {
                Destroy(this);
            }
        }
        
        void Start()
        {
            ResetFence();
        }
        
        public void ResetFence()
        {
            currentWoodHp = maxWoodHp;
            currentSteelHp = maxSteelHp;
            currentState = FenceState.WoodGood;
            UpdateFenceVisuals();
            UpdateBattleManagerHP();
            
            Debug.Log($"[FenceDestruction] Fence reset with Wood HP: {currentWoodHp}, Steel HP: {currentSteelHp}, State: {currentState}");
        }
        
        public void TakeDamage(int damage)
        {
            Debug.Log($"[FenceDestruction] Taking {damage} damage!");
            
            if (currentWoodHp > 0)
            {
                // Damage wood first
                currentWoodHp -= damage;
                currentWoodHp = Mathf.Max(currentWoodHp, 0);
                
                Debug.Log($"[FenceDestruction] Wood HP: {currentWoodHp}/{maxWoodHp}");
                
                if (currentWoodHp <= 0 && currentSteelHp > 0)
                {
                    Debug.Log("[FenceDestruction] Wood fence destroyed! Steel fence exposed!");
                }
            }
            else if (currentSteelHp > 0)
            {
                // Damage steel after wood is gone
                currentSteelHp -= damage;
                currentSteelHp = Mathf.Max(currentSteelHp, 0);
                
                Debug.Log($"[FenceDestruction] Steel HP: {currentSteelHp}/{maxSteelHp}");
                
                if (currentSteelHp <= 0)
                {
                    Debug.Log("[FenceDestruction] COMPLETE FENCE FAILURE! Game Over!");
                    BattleManager.Instance.GameOver(false);
                    return;
                }
            }
            
            UpdateFenceState();
            UpdateFenceVisuals();
            UpdateBattleManagerHP();
        }
        
        void UpdateFenceState()
        {
            FenceState oldState = currentState;
            
            // Wood HP: 4 states, 25% each (75%, 50%, 25%, 0%)
            if (currentWoodHp > maxWoodHp * 0.75f) currentState = FenceState.WoodGood;        // 100% to 75%
            else if (currentWoodHp > maxWoodHp * 0.5f) currentState = FenceState.WoodDamage1; // 75% to 50%  
            else if (currentWoodHp > maxWoodHp * 0.25f) currentState = FenceState.WoodDamage2; // 50% to 25%
            else if (currentWoodHp > 0) currentState = FenceState.WoodDamage3;                 // 25% to 0%
            // Steel HP: 4 states, 25% each (75%, 50%, 25%, 0%)  
            else if (currentSteelHp > maxSteelHp * 0.75f) currentState = FenceState.SteelGood;     // 100% to 75%
            else if (currentSteelHp > maxSteelHp * 0.5f) currentState = FenceState.SteelDamage1;  // 75% to 50%
            else if (currentSteelHp > maxSteelHp * 0.25f) currentState = FenceState.SteelDamage2; // 50% to 25%
            else if (currentSteelHp > 0) currentState = FenceState.SteelDamage3;                  // 25% to 0%
            else currentState = FenceState.Failure;
            
            if (oldState != currentState)
            {
                Debug.Log($"[FenceDestruction] State changed from {oldState} to {currentState}");
            }
        }
        
        void UpdateFenceVisuals()
        {
            // Deactivate all first
            if (Wood_0) Wood_0.SetActive(false);
            if (Wood_1) Wood_1.SetActive(false);
            if (Wood_2) Wood_2.SetActive(false);
            if (Wood_3) Wood_3.SetActive(false);
            if (Steel_0) Steel_0.SetActive(false);
            if (Steel_1) Steel_1.SetActive(false);
            if (Steel_2) Steel_2.SetActive(false);
            if (Steel_3) Steel_3.SetActive(false);
            
            // Activate based on current state
            switch (currentState)
            {
                case FenceState.WoodGood:
                    if (Wood_0) Wood_0.SetActive(true);
                    if (Steel_0) Steel_0.SetActive(true);
                    break;
                case FenceState.WoodDamage1:
                    if (Wood_1) Wood_1.SetActive(true);
                    if (Steel_0) Steel_0.SetActive(true);
                    break;
                case FenceState.WoodDamage2:
                    if (Wood_2) Wood_2.SetActive(true);
                    if (Steel_0) Steel_0.SetActive(true);
                    break;
                case FenceState.WoodDamage3:
                    if (Wood_3) Wood_3.SetActive(true);
                    if (Steel_0) Steel_0.SetActive(true);
                    break;
                case FenceState.SteelGood:
                    if (Steel_0) Steel_0.SetActive(true);
                    break;
                case FenceState.SteelDamage1:
                    if (Steel_1) Steel_1.SetActive(true);
                    break;
                case FenceState.SteelDamage2:
                    if (Steel_2) Steel_2.SetActive(true);
                    break;
                case FenceState.SteelDamage3:
                    if (Steel_3) Steel_3.SetActive(true);
                    break;
                case FenceState.Failure:
                    // All inactive - complete destruction
                    Debug.Log("[FenceDestruction] All fences destroyed!");
                    break;
            }
        }
        
        void UpdateBattleManagerHP()
        {
            // Update BattleManager HP to match fence state
            int totalCurrentHp = currentWoodHp + currentSteelHp;
            int totalMaxHp = maxWoodHp + maxSteelHp;
            
            if (BattleManager.Instance != null && BattleManager.Instance.State != null)
            {
                BattleManager.Instance.State.Hp = totalCurrentHp;
                BattleManager.Instance.State.MaxHp = totalMaxHp;
                BattleManager.Instance.State.HpRate = (float)totalCurrentHp / totalMaxHp;
                
                // Update UI
                if (BattleManager.Instance.hpText != null) 
                    BattleManager.Instance.hpText.text = totalCurrentHp.ToString();
                if (BattleManager.Instance.hpBar != null) 
                    BattleManager.Instance.hpBar.localScale = new Vector3(BattleManager.Instance.State.HpRate, 1, 1);
            }
        }
        
        // Call this from MonsterManager when monsters attack
        public void OnMonsterAttack(int damage)
        {
            TakeDamage(damage);
        }
    }
}