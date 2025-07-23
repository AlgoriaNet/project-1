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
        }
        
        private void OnDetail()
        {
            // Open EquipmentDetailBox for both empty and equipped slots
            Debug.Log($"[EquipmentColumnManager] Equipment slot clicked - equipment: {(_equipment != null ? _equipment.Name : "NULL")}");
            EquipmentDetailBox.Instance.Init(_equipment);
        }
    }
}