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
                    EquipmentDetailManager.EquipmentDetailStatus.Current
                });
            popUpBox.SetActive(true);
        }
    }
}