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
        }

        public void ClearIcon()
        {
            _equipment = null;
            icon.sprite = null;
            icon.color = Color.clear;
        }
        
        private void OnDetail()
        {
            // Only open EquipmentDetailBox if equipment exists (not for empty slots)
            if (_equipment != null)
            {
                Debug.Log($"[EquipmentColumnManager] Equipment slot clicked - equipment: {_equipment.Name}");
                EquipmentDetailBox.Instance.Init(_equipment);
            }
            else
            {
                Debug.Log($"[EquipmentColumnManager] Empty equipment slot clicked - no action (should be handled by AlliesEquipments for empty slots)");
            }
        }
    }
}