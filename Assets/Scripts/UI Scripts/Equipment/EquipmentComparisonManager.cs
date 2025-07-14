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
            if (_currentEquippedOn == EquippedOn.Hero)
            {
                _currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == _comparedEquipment.Part && equipment.EquipWithHeroId > 0);
            }
            else
            {
                _currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == _comparedEquipment.Part && equipment.EquipWithSidekickId == sidekickId);
            }

            SidekickId = sidekickId;
            UpdateUi();
            popUpBox.SetActive(true);
        }

        private void UpdateUi()
        {
            if (_currentEquipment != null)
            {
                currentEquipmentTransform.gameObject.SetActive(true);
                _currentEquipmentDetailManager.Init(_currentEquipment,
                    new List<EquipmentDetailManager.EquipmentDetailStatus>
                    {
                        EquipmentDetailManager.EquipmentDetailStatus.Current
                    });
            }
            else
            {
                currentEquipmentTransform.gameObject.SetActive(false);
            }

            comparedEquipmentTransform.gameObject.SetActive(true);
            _comparedEquipmentDetailManager.Init(_comparedEquipment,
                new List<EquipmentDetailManager.EquipmentDetailStatus>
                {
                    EquipmentDetailManager.EquipmentDetailStatus.Compare,
                    EquipmentDetailManager.EquipmentDetailStatus.Replace,
                    EquipmentDetailManager.EquipmentDetailStatus.Demount
                },
                new EquipmentDetailManager.Info
                {
                    Type = _currentEquippedOn.ToString().ToLower(),
                    SidekickId = _currentEquippedOn == EquippedOn.Sidekick ? SidekickId : null
                });
        }
    }
}