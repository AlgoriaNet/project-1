using System;
using System.Collections.Generic;
using model;
using UnityEngine;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones
{
    public class GemDetailWithInlaid : MonoBehaviour
    {
        [SerializeField] private GemDetail gemDetail;
        [SerializeField] private InlayGemstones inlayGemstones;
        [SerializeField] private GameObject popup;
        private List<Gemstone> _gems;
        public static GemDetailWithInlaid Instance;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            // Removed automatic popup closing on "Bag" changes to allow embedding feedback
            // PlayerProfile.Data.AddListener((arg0 => popup.SetActive(false)), "Bag");
        }

        public void Init(Gemstone gemstone, int? sidekickId)
        {
            gemDetail.Init(gemstone, sidekickId);
            if (sidekickId != null)
            {
                _gems = PlayerProfile.Data.GetSidekickGemstones((int)sidekickId, gemstone.Part);
            }
            else
            {
                _gems = PlayerProfile.Data.GetHeroGemstones(gemstone.Part);
            }
            // Use the new data-driven method
            RefreshInlayGemstones(gemstone.Part, sidekickId);
            popup.SetActive(true);
        }
        
        /// <summary>
        /// Public method to refresh the InlayGemstones UI
        /// </summary>
        public void RefreshInlayGemstones(string equipmentPart, int? sidekickId)
        {
            if (inlayGemstones != null)
            {
                Debug.Log($"[GemDetailWithInlaid] Refreshing InlayGemstones for {equipmentPart} using data-driven approach");
                
                // Find the equipment from player data
                Equipment targetEquipment = null;
                if (sidekickId != null)
                {
                    // For Allies - find equipment equipped to this sidekick
                    targetEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                        equipment.Part == equipmentPart && equipment.EquipWithSidekickId == sidekickId);
                }
                else
                {
                    // For Hero - find equipment equipped to hero
                    targetEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                        equipment.Part == equipmentPart && equipment.EquipWithHeroId > 0);
                }
                
                if (targetEquipment != null)
                {
                    // Use new data-driven approach instead of UI parsing
                    inlayGemstones.InitFromEquipmentData(targetEquipment);
                }
                else
                {
                    Debug.LogWarning($"[GemDetailWithInlaid] No equipped {equipmentPart} found for character");
                    // Fallback to old method if equipment not found
                    inlayGemstones.InitFromDots(equipmentPart, sidekickId);
                }
            }
            else
            {
                Debug.LogWarning($"[GemDetailWithInlaid] inlayGemstones is null, cannot refresh");
            }
        }
    }
}