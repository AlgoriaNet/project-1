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
            Demount
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
            forgeButton.onClick.AddListener(OnForge);
            replaceButton.onClick.AddListener(OnReplace);
            demountButton.onClick.AddListener(OnDemount);
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
                detailStatus == EquipmentDetailStatus.Demount));

            // Assign sprite to the Image based on resourcesPath
            Sprite itemSprite = Resources.Load<Sprite>($"UILoading/Equipment/{_equipment.Name}");
            equipmentName.text = _equipment.Name;
            background.color = ItemLoader.quantityColor[_equipment.Quality];
            icon.sprite = itemSprite;
            icon.color = Color.white;
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

        public void OnReplace()
        {
            Debug.Log("OnReplace");
            if (_info == null)
            {
                //todo 提示Error
                return;
            }

            var apiParams = new
            {
                type = _info.Type,
                sidekickId = _info.SidekickId,
                equipmentId = _equipment.Id
            };
            _equipmentApi.Action("replace", apiParams, SetProfileFromServer);
        }

        public void OnDemount()
        {
        }

        public void OnForge()
        {
        }

        private void SetProfileFromServer(JObject obj)
        {
            PlayerProfile.Data.SetPlayer(obj["Player"].ToObject<Player>());
        }
    }
}