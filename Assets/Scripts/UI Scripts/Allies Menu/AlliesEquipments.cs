using System.Collections.Generic;
using System.Linq;
using model;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox;
using UnityEngine;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Allies_Menu
{
    public class AlliesEquipments : MonoBehaviour
    {
        [SerializeField] private Transform helmTransform;
        [SerializeField] private Transform shoulderTransform;
        [SerializeField] private Transform chestTransform;
        [SerializeField] private Transform pantsTransform;
        [SerializeField] private Transform glovesTransform;
        [SerializeField] private Transform bootsTransform;
        
        // Reference to AlliesGridSetup to get current sidekick information
        [SerializeField] private AlliesGridSetup alliesGridSetup;

        public void Start()
        {
            // Don't init immediately - wait for ally selection
            // Clear equipment display initially
            ClearEquipmentDisplay();
            PlayerProfile.Data.AddListener(UpdateUI, "Player");
        }
        
        private void OnDestroy()
        {
            // Clean up listeners
            if (PlayerProfile.Data != null)
            {
                PlayerProfile.Data.RemoveListener(UpdateUI, "Player");
            }
        }

        /// <summary>
        /// Initialize equipment display for the current ally
        /// Called when an ally is selected in Step 2
        /// </summary>
        public void InitForCurrentAlly()
        {
            // Get current sidekick ID
            int currentSidekickId = GetCurrentSidekickId();
            if (currentSidekickId == 0)
            {
                ClearEquipmentDisplay();
                return;
            }

            InitForSpecificSidekick(currentSidekickId);
        }

        /// <summary>
        /// Initialize equipment display for a specific sidekick ID
        /// Used for refreshing equipment after auto embed operations
        /// </summary>
        public void InitForSpecificSidekick(int sidekickId)
        {
            if (sidekickId == 0)
            {
                Debug.LogWarning("[AlliesEquipments] Invalid sidekick ID 0 - clearing equipment display");
                ClearEquipmentDisplay();
                return;
            }

            // Get equipments for this specific sidekick
            List<Equipment> sidekickEquipments = PlayerProfile.Data.GetSidekickEquipments(sidekickId);
            
            // Clear existing equipment display
            ClearEquipmentDisplay();
            
            // Track which slots have equipment
            bool[] hasEquipment = new bool[6]; // helm, shoulder, chest, pants, gloves, boots
            
            // Load equipment into slots
            foreach (var equipment in sidekickEquipments)
            {
                switch (equipment.Part)
                {
                    case "Helm":
                        InitEquipmentSlot(helmTransform, equipment, sidekickId);
                        hasEquipment[0] = true;
                        break;
                    case "Shoulder":
                        InitEquipmentSlot(shoulderTransform, equipment, sidekickId);
                        hasEquipment[1] = true;
                        break;
                    case "Chest":
                        InitEquipmentSlot(chestTransform, equipment, sidekickId);
                        hasEquipment[2] = true;
                        break;
                    case "Pants":
                        InitEquipmentSlot(pantsTransform, equipment, sidekickId);
                        hasEquipment[3] = true;
                        break;
                    case "Gloves":
                        InitEquipmentSlot(glovesTransform, equipment, sidekickId);
                        hasEquipment[4] = true;
                        break;
                    case "Boots":
                        InitEquipmentSlot(bootsTransform, equipment, sidekickId);
                        hasEquipment[5] = true;
                        break;
                }
            }
            
            // Initialize empty slots
            Transform[] slotTransforms = { helmTransform, shoulderTransform, chestTransform, pantsTransform, glovesTransform, bootsTransform };
            string[] equipmentParts = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
            for (int i = 0; i < slotTransforms.Length; i++)
            {
                if (!hasEquipment[i] && slotTransforms[i] != null)
                {
                    InitEmptyEquipmentSlot(slotTransforms[i], equipmentParts[i], sidekickId);
                }
            }
        }

        /// <summary>
        /// Initialize a specific equipment slot with click handler for comparison
        /// </summary>
        private void InitEquipmentSlot(Transform slotTransform, Equipment equipment, int sidekickId)
        {
            // Get or add EquipmentColumnManager component
            EquipmentColumnManager columnManager = slotTransform.GetComponent<EquipmentColumnManager>();
            if (columnManager == null)
            {
                Debug.LogWarning($"[AlliesEquipments] EquipmentColumnManager not found on {slotTransform.name}");
                return;
            }

            // Initialize the equipment display
            columnManager.Init(equipment, new List<Gemstone>());
            
            // Set up click handler to open EquipmentDetailBox (same as Hero menu behavior)
            if (columnManager.detailButton != null)
            {
                columnManager.detailButton.onClick.RemoveAllListeners();
                columnManager.detailButton.onClick.AddListener(() => 
                {
                    EquipmentDetailBox.Instance.Init(equipment);
                });
            }
        }

        /// <summary>
        /// Initialize an empty equipment slot (no equipment equipped)
        /// </summary>
        private void InitEmptyEquipmentSlot(Transform slotTransform, string equipmentPart, int sidekickId)
        {
            // Get or add EquipmentColumnManager component
            EquipmentColumnManager columnManager = slotTransform.GetComponent<EquipmentColumnManager>();
            if (columnManager == null)
            {
                Debug.LogWarning($"[AlliesEquipments] EquipmentColumnManager not found on {slotTransform.name}");
                return;
            }

            // Keep the click handler for empty slots - show equipment selection for that part
            if (columnManager.detailButton != null)
            {
                columnManager.detailButton.onClick.RemoveAllListeners();
                columnManager.detailButton.onClick.AddListener(() => 
                {
                    ShowEmptySlotMessage(equipmentPart, sidekickId);
                });
            }
        }

        /// <summary>
        /// Show a message or UI for empty equipment slot clicks
        /// </summary>
        private void ShowEmptySlotMessage(string equipmentPart, int sidekickId)
        {
            // TODO: Implement equipment selection popup for this part type
            // For now, maybe just log or show a simple message
        }

        /// <summary>
        /// Clear all equipment displays when no ally is selected
        /// </summary>
        private void ClearEquipmentDisplay()
        {
            ClearEquipmentSlot(helmTransform);
            ClearEquipmentSlot(shoulderTransform);
            ClearEquipmentSlot(chestTransform);
            ClearEquipmentSlot(pantsTransform);
            ClearEquipmentSlot(glovesTransform);
            ClearEquipmentSlot(bootsTransform);
        }

        /// <summary>
        /// Clear a specific equipment slot
        /// </summary>
        private void ClearEquipmentSlot(Transform slotTransform)
        {
            if (slotTransform == null) return;
            
            EquipmentColumnManager columnManager = slotTransform.GetComponent<EquipmentColumnManager>();
            if (columnManager != null)
            {
                columnManager.ClearIcon();
                if (columnManager.detailButton != null)
                {
                    columnManager.detailButton.onClick.RemoveAllListeners();
                }
            }
        }

        /// <summary>
        /// Get the current sidekick ID from AlliesGridSetup
        /// </summary>
        private int GetCurrentSidekickId()
        {
            // If AlliesGridSetup is not assigned, try to find it
            if (alliesGridSetup == null)
            {
                alliesGridSetup = FindObjectOfType<AlliesGridSetup>();
            }

            if (alliesGridSetup == null)
            {
                Debug.LogWarning("[AlliesEquipments] AlliesGridSetup not found - cannot determine current sidekick ID");
                return 0;
            }

            // Get current ally name from AlliesGridSetup using reflection
            string currentAllyName = GetCurrentAllyNameFromGridSetup();
            if (string.IsNullOrEmpty(currentAllyName))
            {
                Debug.LogWarning("[AlliesEquipments] Current ally name is empty - using default sidekick ID 0");
                return 0;
            }

            // Convert ally name to sidekick ID by looking up in PlayerProfile data
            if (PlayerProfile.Data?.Sidekick != null)
            {
                // Find the sidekick that matches this ally
                var sidekick = PlayerProfile.Data.Sidekick.FirstOrDefault(s =>
                {
                    // Match by base_id - convert ally name to base_id format
                    string allyBaseId = GetAllyBaseIdFromName(currentAllyName);
                    return s.base_id == allyBaseId;
                });

                if (sidekick != null)
                {
                    if (int.TryParse(sidekick.id, out int sidekickId))
                    {
                        return sidekickId;
                    }
                    else
                    {
                        Debug.LogWarning($"[AlliesEquipments] Could not parse sidekick ID '{sidekick.id}' to int");
                        return 0;
                    }
                }
                else
                {
                    Debug.LogWarning($"[AlliesEquipments] No sidekick found for ally {currentAllyName}");
                }
            }

            return 0; // Default to 0 if not found
        }

        /// <summary>
        /// Get the current ally name from AlliesGridSetup using reflection
        /// </summary>
        private string GetCurrentAllyNameFromGridSetup()
        {
            if (alliesGridSetup == null) return "";

            try
            {
                // Use reflection to access the private currentAllyName field
                var currentAllyNameField = typeof(AlliesGridSetup).GetField("currentAllyName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (currentAllyNameField != null)
                {
                    string currentAllyName = (string)currentAllyNameField.GetValue(alliesGridSetup);
                    return currentAllyName ?? "";
                }
                else
                {
                    Debug.LogWarning("[AlliesEquipments] Could not access currentAllyName field via reflection");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AlliesEquipments] Error getting current ally name: {ex.Message}");
            }

            return "";
        }

        /// <summary>
        /// Convert ally name to base_id format used in sidekick data
        /// </summary>
        private string GetAllyBaseIdFromName(string allyName)
        {
            // Character names matching the file names (same as in AlliesGridSetup)
            string[] characterNames = {
                "Zorath", "Gideon", "Sylas", "Aurelia", "Lyanna", "Zhara", "Elenya", "Rowan",
                "Liraen", "Cedric", "Selena", "Morgath", "Zyphira", "Kaelith", "Velan", "Ragnar",
                "Lucien", "Ugra", "Eleanor", "Nyx"
            };

            for (int i = 0; i < characterNames.Length; i++)
            {
                if (characterNames[i] == allyName)
                {
                    // Convert 0-based index to 1-based base_id (e.g., index 3 → base_id "4")
                    return (i + 1).ToString();
                }
            }

            Debug.LogWarning($"[AlliesEquipments] Unknown ally name: {allyName}");
            return "0";
        }

        private void UpdateUI(ApplicationModel model)
        {
            // Refresh equipment display when player data changes
            InitForCurrentAlly();
        }
    }
}