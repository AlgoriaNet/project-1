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
    public class EquipmentDetailManager : MonoBehaviour
    {
        public enum EquipmentDetailStatus
        {
            Current,
            Compare,
            Forge,
            Replace,
            Dismantle,
            Equip
        }
        
        [NonSerialized] public List<EquipmentDetailStatus> Status;
        [SerializeField] public Transform currentTag;
        [SerializeField] public TextMeshProUGUI equipmentName;
        [SerializeField] public Image background;
        [SerializeField] public Image icon;
        [SerializeField] public TextMeshProUGUI attack;
        [SerializeField] public Button forgeButton;
        [SerializeField] public Button replaceButton;
        [SerializeField] public Button demountButton;
        [SerializeField] public Button equipButton;
        [SerializeField] public GameObject extraStatsNames;
        [SerializeField] public GameObject extraStatsValues;
        [NonSerialized] private Equipment _equipment;
        [NonSerialized] private Info _info;
        private EquipmentWebSocketApi _equipmentApi;
        [SerializeField] private List<TextMeshProUGUI> extraNames;
        [SerializeField] private List<TextMeshProUGUI> extraValues;

        public class Info
        {
            public string Type;
            public int? SidekickId;
        }

        public void Awake()
        {
            _equipmentApi = EquipmentWebSocketApi.Instance;
        }
        
        public void Start()
        {
            if (forgeButton != null)
                forgeButton.onClick.AddListener(OnForge);
            if (replaceButton != null)
                replaceButton.onClick.AddListener(OnReplace);
            if (demountButton != null)
                demountButton.onClick.AddListener(OnDismantle);
            if (equipButton != null)
                equipButton.onClick.AddListener(OnEquip);
        }

        public void Init(Equipment equipment, List<EquipmentDetailStatus> status, [CanBeNull] Info info = null)
        {
            _equipment = equipment;
            Status = status;
            _info = info;
            currentTag.gameObject.SetActive(
                status.Exists(detailStatus => detailStatus == EquipmentDetailStatus.Current));
            replaceButton.gameObject.SetActive(status.Exists(detailStatus =>
                detailStatus == EquipmentDetailStatus.Replace));
            demountButton.gameObject.SetActive(status.Exists(detailStatus =>
                detailStatus == EquipmentDetailStatus.Dismantle));
            if (equipButton != null)
                equipButton.gameObject.SetActive(status.Exists(detailStatus =>
                    detailStatus == EquipmentDetailStatus.Equip));

            // Assign sprite to the Image based on resourcesPath
            if (_equipment?.Name != null)
            {
                Sprite itemSprite = Resources.Load<Sprite>($"UILoading/Equipment/{_equipment.Name}");
                icon.sprite = itemSprite;
                equipmentName.text = _equipment.Name;
                background.color = ItemLoader.quantityColor[_equipment.Quality];
                icon.color = Color.white;
            }
            else
            {
                Debug.LogWarning("[EquipmentDetailManager] Equipment or equipment name is null");
                icon.sprite = null;
                equipmentName.text = "No Equipment";
                background.color = Color.gray;
                icon.color = Color.white;
            }
            if (_equipment != null)
            {
                attack.text = _equipment.Attack.ToString();

                for (int i = 0; i < extraNames.Count; i++)
                {
                    if(i < _equipment.NearbyAttributes.Count)
                    {
                        extraNames[i].text = _equipment.NearbyAttributes.Keys.ToArray()[i];
                        extraValues[i].text = _equipment.NearbyAttributes.Values.ToArray()[i].ToString();
                    }
                    else
                    {
                        extraNames[i].text = "";
                        extraValues[i].text = "";
                    }
                }
            }
            else
            {
                attack.text = "0";
                for (int i = 0; i < extraNames.Count; i++)
                {
                    extraNames[i].text = "";
                    extraValues[i].text = "";
                }
            }
        }

        public void OnReplace()
        {
            Debug.Log($"[EquipmentDetailManager] OnReplace called");
            Debug.Log($"[EquipmentDetailManager] _info null: {_info == null}");
            Debug.Log($"[EquipmentDetailManager] _info.Type: {_info?.Type}");
            Debug.Log($"[EquipmentDetailManager] _info.SidekickId: {_info?.SidekickId}");
            Debug.Log($"[EquipmentDetailManager] _equipment null: {_equipment == null}");
            Debug.Log($"[EquipmentDetailManager] _equipment.Id: {_equipment?.Id}");
            
            if (_info == null)
            {
                Debug.LogError("[EquipmentDetailManager] ❌ Replace button clicked but _info is null! Cannot proceed with replace operation.");
                return;
            }

            var apiParams = new
            {
                type = _info.Type,
                sidekickId = _info.SidekickId,
                equipmentId = _equipment.Id
            };
            
            Debug.Log($"[EquipmentDetailManager] Replace API params - type: {_info.Type}, sidekickId: {_info.SidekickId}, equipmentId: {_equipment.Id}");
            _equipmentApi.Action("replace", apiParams, SetProfileFromServer);
        }

        public void OnDismantle()
        {
            if (EquipmentDismantleManager.Instance != null)
            {
                // Determine context based on available info
                EquipmentDismantleManager.DismantleContext context = EquipmentDismantleManager.DismantleContext.Hero;
                
                Debug.Log($"[EquipmentDetailManager] OnDismantle: _info.Type = {_info?.Type}, SidekickId = {_info?.SidekickId}");
                
                if (_info != null && _info.Type == "sidekick")
                {
                    context = EquipmentDismantleManager.DismantleContext.Ally;
                    Debug.Log("[EquipmentDetailManager] Detected Ally context for dismantle!");
                }
                else
                {
                    Debug.Log("[EquipmentDetailManager] Using Hero context for dismantle (default)");
                }
                
                EquipmentDismantleManager.Instance.OpenDismantlePage(context);
            }
            else
            {
                Debug.LogError("❌ EquipmentDismantleManager.Instance is null!");
            }
        }

        public void OnForge()
        {
            if (EquipmentForgeManager.Instance != null)
            {
                // Determine context based on available info
                EquipmentForgeManager.ForgeContext context = EquipmentForgeManager.ForgeContext.Hero;
                
                Debug.Log($"[EquipmentDetailManager] OnForge: _info.Type = {_info?.Type}, SidekickId = {_info?.SidekickId}");
                
                if (_info != null && _info.Type == "sidekick")
                {
                    context = EquipmentForgeManager.ForgeContext.Ally;
                    Debug.Log("[EquipmentDetailManager] Detected Ally context!");
                }
                else
                {
                    Debug.Log("[EquipmentDetailManager] Using Hero context (default)");
                }
                
                EquipmentForgeManager.Instance.OpenForgePage(context);
            }
            else
            {
                Debug.LogError("EquipmentForgeManager.Instance is null!");
            }
        }

        public void OnEquip()
        {
            Debug.Log($"[EquipmentDetailManager] OnEquip called - equipment: {_equipment?.Name}");
            Debug.Log($"[EquipmentDetailManager] _info null: {_info == null}");
            Debug.Log($"[EquipmentDetailManager] _info.Type: {_info?.Type}");
            Debug.Log($"[EquipmentDetailManager] _info.SidekickId: {_info?.SidekickId}");
            Debug.Log($"[EquipmentDetailManager] _equipment null: {_equipment == null}");
            Debug.Log($"[EquipmentDetailManager] _equipment.Id: {_equipment?.Id}");
            
            if (_equipment == null)
            {
                Debug.LogError("[EquipmentDetailManager] OnEquip: No equipment to equip!");
                return;
            }

            if (_info == null)
            {
                Debug.LogError("[EquipmentDetailManager] ❌ Equip button clicked but _info is null! Cannot proceed with equip operation.");
                return;
            }

            var apiParams = new
            {
                type = _info.Type,
                sidekickId = _info.SidekickId,
                equipmentId = _equipment.Id
            };
            
            Debug.Log($"[EquipmentDetailManager] Equip API params - type: {_info.Type}, sidekickId: {_info.SidekickId}, equipmentId: {_equipment.Id}");
            Debug.Log($"[EquipmentDetailManager] Using Replace API for equipping to empty slot");
            
            // Use the existing Replace API - it handles both replace and equip scenarios
            _equipmentApi.Action("replace", apiParams, SetProfileFromServer);
        }

        private void SetProfileFromServer(JObject obj)
        {
            PlayerProfile.Data.SetPlayer(obj["Player"].ToObject<Player>());
        }
    }
}