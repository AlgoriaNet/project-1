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
        // [SerializeField] public Button replaceButton;
        // [SerializeField] public Button demountButton;
        [SerializeField] public Button equipButton;
        [SerializeField] public GameObject extraStatsNames;
        [SerializeField] public GameObject extraStatsValues;
        [NonSerialized] private Equipment _equipment;
        [NonSerialized] private Info _info;
        private EquipmentWebSocketApi _equipmentApi;
        private bool _isEquipping = false; // Guard to prevent double equipping
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
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] Start called - adding button listeners");
            
            if (forgeButton != null)
                forgeButton.onClick.AddListener(OnForge);
            // if (replaceButton != null)
            //     replaceButton.onClick.AddListener(OnReplace);
            // if (demountButton != null)
            //     demountButton.onClick.AddListener(OnDismantle);
            // Don't add equipButton listener here - it will be added in Init() to prevent conflicts
            // if (equipButton != null)
            //     equipButton.onClick.AddListener(OnEquip);
        }

        public void Init(Equipment equipment, List<EquipmentDetailStatus> status, [CanBeNull] Info info = null)
        {
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] Init called - equipment: {equipment?.Name}, info.Type: {info?.Type}, info.SidekickId: {info?.SidekickId}");
            
            _equipment = equipment;
            Status = status;
            _info = info;
            
            // Remove and re-add listeners to prevent double-clicking issues
            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(OnEquip);
                Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] Added OnEquip listener to equipButton");
            }
            else
            {
                Debug.LogWarning($"[EquipmentDetailManager-{GetInstanceID()}] equipButton is null in Init!");
            }
            currentTag.gameObject.SetActive(
                status.Exists(detailStatus => detailStatus == EquipmentDetailStatus.Current));
            // replaceButton.gameObject.SetActive(status.Exists(detailStatus =>
            //     detailStatus == EquipmentDetailStatus.Replace));
            // demountButton.gameObject.SetActive(status.Exists(detailStatus =>
            //     detailStatus == EquipmentDetailStatus.Dismantle));
            if (equipButton != null)
                equipButton.gameObject.SetActive(status.Exists(detailStatus =>
                    detailStatus == EquipmentDetailStatus.Equip));

            // Assign sprite to the Image based on resourcesPath
            if (_equipment?.Name != null)
            {
                Sprite itemSprite = Resources.Load<Sprite>($"UILoading/Equipment/{_equipment.Name}");
                icon.sprite = itemSprite;
                equipmentName.text = !string.IsNullOrEmpty(_equipment.DisplayName) 
                    ? _equipment.DisplayName 
                    : _equipment.Name;
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
                
                EquipmentForgeManager.Instance.OpenForgePage(_equipment, context);
            }
            else
            {
                Debug.LogError("EquipmentForgeManager.Instance is null!");
            }
        }

        public void OnEquip()
        {
            // Prevent double execution
            if (_isEquipping)
            {
                Debug.LogWarning($"[EquipmentDetailManager-{GetInstanceID()}] OnEquip already in progress - ignoring duplicate call");
                return;
            }
            
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] OnEquip called - equipment: {_equipment?.Name}");
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] _info null: {_info == null}");
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] _info.Type: {_info?.Type}");
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] _info.SidekickId: {_info?.SidekickId}");
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] _equipment null: {_equipment == null}");
            Debug.Log($"[EquipmentDetailManager-{GetInstanceID()}] _equipment.Id: {_equipment?.Id}");
            
            if (_equipment == null)
            {
                Debug.LogWarning($"[EquipmentDetailManager-{GetInstanceID()}] OnEquip: No equipment data - likely old instance with stale listener, ignoring");
                return;
            }

            if (_info == null)
            {
                Debug.LogError("[EquipmentDetailManager] ❌ Equip button clicked but _info is null! Cannot proceed with equip operation.");
                return;
            }

            // Set the guard flag and disable button
            _isEquipping = true;
            if (equipButton != null)
            {
                equipButton.interactable = false;
                Debug.Log("[EquipmentDetailManager] Disabled equip button to prevent double-clicking");
            }

            var apiParams = new
            {
                type = _info.Type,
                sidekickId = _info.SidekickId,
                equipmentId = _equipment.Id
            };
            
            Debug.Log($"[EquipmentDetailManager] Equip API params - type: {_info.Type}, sidekickId: {_info.SidekickId}, equipmentId: {_equipment.Id}");
            Debug.Log($"[EquipmentDetailManager] Using dedicated Equip API for empty slot equipping");
            
            // Use the dedicated Equip API for empty slot equipping
            _equipmentApi.Action("equip", apiParams, SetProfileFromServer);
        }

        private void SetProfileFromServer(JObject obj)
        {
            Debug.Log($"[EquipmentDetailManager] SetProfileFromServer called with response structure");
            
            // Reset the guard flag and re-enable the equip button regardless of success/failure
            _isEquipping = false;
            if (equipButton != null)
            {
                equipButton.interactable = true;
                Debug.Log("[EquipmentDetailManager] Re-enabled equip button and reset guard flag");
            }
            
            if (obj == null)
            {
                Debug.LogError("[EquipmentDetailManager] ❌ Server response is null!");
                return;
            }
            
            // Check for the correct response structure: obj["player_profile"]["Player"]
            if (obj["player_profile"] == null)
            {
                Debug.LogError($"[EquipmentDetailManager] ❌ Server response missing 'player_profile' field. Response keys: {string.Join(", ", obj.Properties().Select(p => p.Name))}");
                return;
            }
            
            if (obj["player_profile"]["Player"] == null)
            {
                Debug.LogError($"[EquipmentDetailManager] ❌ Server response missing 'Player' field in player_profile. player_profile keys: {string.Join(", ", obj["player_profile"].Cast<JProperty>().Select(p => p.Name))}");
                return;
            }
            
            try
            {
                PlayerProfile.Data.SetPlayer(obj["player_profile"]["Player"].ToObject<Player>());
                Debug.Log("[EquipmentDetailManager] ✅ Player profile updated successfully from server response");
                
                // Close the equipment detail popup after successful equipping
                if (EquipmentDetailBox.Instance != null && EquipmentDetailBox.Instance.popUpBox != null)
                {
                    EquipmentDetailBox.Instance.popUpBox.SetActive(false);
                    Debug.Log("[EquipmentDetailManager] Closed equipment detail popup after successful equipping");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EquipmentDetailManager] ❌ Failed to parse Player object: {ex.Message}");
            }
        }
    }
}