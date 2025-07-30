using System;
using System.Collections.Generic;
using model;
using UnityEngine;
using UnityEngine.UI;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox
{
    // 装备栏管理器
    public class EquipmentColumnManager :MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private List<Image> gems;
        [SerializeField] public Button detailButton;
        private Equipment _equipment;

        public void Start()
        {
            detailButton.onClick.AddListener(OnDetail);
        }

        public void Init(Equipment equipment, List<model.Gemstone> gemstones)
        {
            _equipment = equipment;
            background.color = ItemLoader.quantityColor[equipment.Quality];
            icon.sprite = Resources.Load<Sprite>($"UILoading/Equipment/{equipment.Name}");
            icon.color = Color.white;

            // Initialize gems/dots from equipment embedded gems data
            InitializeGemSlots(equipment);
        }

        /// <summary>
        /// Initialize gem slots (dots) from equipment embedded gems data
        /// </summary>
        private void InitializeGemSlots(Equipment equipment)
        {
            if (gems == null || gems.Count == 0)
            {
                return;
            }

            // Initialize all gem slots with default Dot_00 first
            Sprite defaultDotSprite = Resources.Load<Sprite>("UILoading/Gem/Dots/Dot_00");
            for (int i = 0; i < gems.Count; i++)
            {
                if (gems[i] != null)
                {
                    gems[i].sprite = defaultDotSprite;
                    gems[i].color = Color.white;
                }
            }

            // Populate gem slots from equipment embedded gems data
            if (equipment?.EmbeddedGems != null && equipment.EmbeddedGems.Count > 0)
            {
                foreach (var embeddedGem in equipment.EmbeddedGems)
                {
                    if (embeddedGem.gem == null || embeddedGem.is_empty)
                    {
                        continue; // Skip empty slots
                    }
                    
                    int slotIndex = embeddedGem.slot - 1; // Convert 1-based to 0-based
                    if (slotIndex >= 0 && slotIndex < gems.Count)
                    {
                        // Load the appropriate dot sprite based on gem level
                        string dotSpriteName = $"Dot_{embeddedGem.gem.Level:D2}";
                        Sprite dotSprite = Resources.Load<Sprite>($"UILoading/Gem/Dots/{dotSpriteName}");
                        
                        if (dotSprite != null)
                        {
                            gems[slotIndex].sprite = dotSprite;
                            gems[slotIndex].color = Color.white;
                        }
                    }
                }
            }
        }

        public void ClearIcon()
        {
            _equipment = null;
            icon.sprite = null;
            icon.color = Color.clear;
            // Reset background color to white for empty slots
            background.color = Color.white;

            // Reset gem slots to default Dot_00
            if (gems != null)
            {
                Sprite defaultDotSprite = Resources.Load<Sprite>("UILoading/Gem/Dots/Dot_00");
                for (int i = 0; i < gems.Count; i++)
                {
                    if (gems[i] != null)
                    {
                        gems[i].sprite = defaultDotSprite;
                        gems[i].color = Color.white;
                    }
                }
            }
        }
        
        private void OnDetail()
        {
            // Only open EquipmentDetailBox if equipment exists (not for empty slots)
            if (_equipment != null)
            {
                EquipmentDetailBox.Instance.Init(_equipment);
            }
            // else: do nothing for empty slot (handled elsewhere)
        }
    }
}