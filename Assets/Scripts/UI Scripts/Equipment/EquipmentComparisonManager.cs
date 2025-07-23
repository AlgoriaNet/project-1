using System;
using System.Collections.Generic;
using model;
using UnityEngine;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox
{
    public class EquipmentComparisonManager : MonoBehaviour
    {
        public enum EquippedOn
        {
            Hero,
            Sidekick
        }

        [NonSerialized] private EquippedOn _currentEquippedOn;
        [NonSerialized] public int ComparedEquippedId;
        [NonSerialized] public int SidekickId;
        private Equipment _currentEquipment;
        private Equipment _comparedEquipment;
        [SerializeField] private Transform currentEquipmentTransform;
        [SerializeField] private Transform comparedEquipmentTransform;
        private EquipmentDetailManager _currentEquipmentDetailManager;
        private EquipmentDetailManager _comparedEquipmentDetailManager;
        public static EquipmentComparisonManager Instance;
        [SerializeField] private GameObject popUpBox;


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
            _comparedEquipmentDetailManager = comparedEquipmentTransform.GetComponent<EquipmentDetailManager>();
            PlayerProfile.Data.AddListener((arg0 => popUpBox.SetActive(false)), "Player");
        }

        public void Init(EquippedOn equippedOn, int? equippedId, int sidekickId = 0)
        {
            if (equippedId == null)
            {
                return;
            }

            _currentEquippedOn = equippedOn;
            ComparedEquippedId = (int)equippedId;
            _comparedEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment => equipment.Id == equippedId);
            
            Debug.Log($"[EquipmentComparisonManager] Init - looking for compared equipment ID {equippedId}");
            Debug.Log($"[EquipmentComparisonManager] Init - found compared equipment: {(_comparedEquipment != null ? $"ID {_comparedEquipment.Id}, Part {_comparedEquipment.Part}" : "NULL")}");
            
            if (_currentEquippedOn == EquippedOn.Hero)
            {
                Debug.Log("[EquipmentComparisonManager] Init - looking for current Hero equipment");
                _currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == _comparedEquipment.Part && equipment.EquipWithHeroId > 0);
            }
            else
            {
                Debug.Log($"[EquipmentComparisonManager] Init - looking for current Sidekick equipment with sidekickId {sidekickId} and part {_comparedEquipment?.Part}");
                _currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == _comparedEquipment.Part && equipment.EquipWithSidekickId == sidekickId);
                Debug.Log($"[EquipmentComparisonManager] Init - found current sidekick equipment: {(_currentEquipment != null ? $"ID {_currentEquipment.Id}" : "NULL")}");
            }

            SidekickId = sidekickId;
            UpdateUi();
            popUpBox.SetActive(true);
        }

        private void UpdateUi()
        {
            Debug.Log($"[EquipmentComparisonManager] UpdateUi - currentEquipment: {(_currentEquipment != null ? $"Found ID {_currentEquipment.Id}" : "NULL")}");
            Debug.Log($"[EquipmentComparisonManager] UpdateUi - comparedEquipment: {(_comparedEquipment != null ? $"Found ID {_comparedEquipment.Id}" : "NULL")}");
            Debug.Log($"[EquipmentComparisonManager] UpdateUi - currentEquippedOn: {_currentEquippedOn}, SidekickId: {SidekickId}");
            
            // Always show EquipmentDetail (1) - even for empty slots
            Debug.Log($"[EquipmentComparisonManager] Always activating currentEquipmentTransform - equipment: {(_currentEquipment != null ? $"ID {_currentEquipment.Id}" : "NULL")}");
            currentEquipmentTransform.gameObject.SetActive(true);
            _currentEquipmentDetailManager.Init(_currentEquipment,
                new List<EquipmentDetailManager.EquipmentDetailStatus>
                {
                    EquipmentDetailManager.EquipmentDetailStatus.Current
                });

            comparedEquipmentTransform.gameObject.SetActive(true);
            var infoObject = new EquipmentDetailManager.Info
            {
                Type = _currentEquippedOn.ToString().ToLower(),
                SidekickId = _currentEquippedOn == EquippedOn.Sidekick ? SidekickId : null
            };
            
            Debug.Log($"[EquipmentComparisonManager] Creating Info object - Type: {infoObject.Type}, SidekickId: {infoObject.SidekickId}");
            
            _comparedEquipmentDetailManager.Init(_comparedEquipment,
                new List<EquipmentDetailManager.EquipmentDetailStatus>
                {
                    EquipmentDetailManager.EquipmentDetailStatus.Compare,
                    EquipmentDetailManager.EquipmentDetailStatus.Replace,
                    EquipmentDetailManager.EquipmentDetailStatus.Forge,
                    EquipmentDetailManager.EquipmentDetailStatus.Dismantle
                },
                infoObject);
        }
    }
}