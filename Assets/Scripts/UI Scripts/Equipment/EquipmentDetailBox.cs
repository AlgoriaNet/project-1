using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using model;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WebSocket;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox
{
    // 装备详情页面
    // 通过传入的装备信息，初始化装备详情页面
    // 通过传入的状态信息，初始化装备详情页面的状态
    public class EquipmentDetailBox : MonoBehaviour
    {
        [SerializeField] public GameObject popUpBox;
        [SerializeField] private Transform currentEquipmentTransform;
        private EquipmentDetailManager _currentEquipmentDetailManager;
        public static EquipmentDetailBox Instance;
        
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
        }
        
        public void Start()
        {
            popUpBox.SetActive(false);
            _currentEquipmentDetailManager = currentEquipmentTransform.GetComponent<EquipmentDetailManager>();
            PlayerProfile.Data.AddListener((arg0 => popUpBox.SetActive(false)), "Player");
        }
        
        public void Init(Equipment equipment)
        {
            _currentEquipmentDetailManager.Init(equipment,
                new List<EquipmentDetailManager.EquipmentDetailStatus>
                {
                    EquipmentDetailManager.EquipmentDetailStatus.Current,
                    EquipmentDetailManager.EquipmentDetailStatus.Forge // Show Forge button for equipped items (upper group)
                },
                null); // Add the missing third parameter
            popUpBox.SetActive(true);
        }

        /// <summary>
        /// Initialize equipment detail box for equipping to an empty slot
        /// Shows Replace button for equipping functionality
        /// </summary>
        /// <param name="equipment">The equipment to show</param>
        /// <param name="context">The context (Hero or Sidekick)</param>
        /// <param name="contextId">The context ID (0 for Hero, sidekick ID for Sidekick)</param>
        public void InitForEquipping(Equipment equipment, EquipmentComparisonManager.EquippedOn context, int contextId)
        {
            var infoObject = new EquipmentDetailManager.Info
            {
                Type = context.ToString().ToLower(),
                SidekickId = context == EquipmentComparisonManager.EquippedOn.Sidekick ? contextId : (int?)null
            };

            Debug.Log($"[EquipmentDetailBox] InitForEquipping - equipment: {equipment?.Name}, context: {context}, contextId: {contextId}");
            Debug.Log($"[EquipmentDetailBox] Created Info object - Type: {infoObject.Type}, SidekickId: {infoObject.SidekickId}");

            _currentEquipmentDetailManager.Init(equipment,
                new List<EquipmentDetailManager.EquipmentDetailStatus>
                {
                    EquipmentDetailManager.EquipmentDetailStatus.Current,
                    EquipmentDetailManager.EquipmentDetailStatus.Equip // Show Equip button for empty slots
                },
                infoObject);
            popUpBox.SetActive(true);
        }
    }
}