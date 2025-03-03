using System;
using System.Collections.Generic;
using System.Globalization;
using model;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocket;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones
{
    public class GemDetail : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI name;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI level;
        [SerializeField] private TextMeshProUGUI part;
        [SerializeField] private TextMeshProUGUI value;
        [SerializeField] Button inlayButton;
        private Gemstone _gemstone;
        private int? _sidekickId;
        private GemWebSocketApi _gemApi;

        public void Awake()
        {
            _gemApi = GemWebSocketApi.Instance;
            inlayButton.onClick.AddListener(OnInlay);
        }

        public void Init(Gemstone gemstone, int? sidekickId)
        {
            _gemstone = gemstone;
            _sidekickId = sidekickId;
            name.text = gemstone.Name;
            icon.sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstone.Level:D2}");
            description.text = gemstone.Description;
            level.text = gemstone.Level.ToString();
            value.text = gemstone.EntryValue.ToString(CultureInfo.InvariantCulture);
            part.text = gemstone.Part;
        }
        
        private void OnInlay()
        {
             var apiParams = new
            {
                gemId = _gemstone.Id,
                sidekickId = _sidekickId,
            };
            _gemApi.Action("inlay", apiParams, SetProfileFromServer);
        }

        private void SetProfileFromServer(JObject obj)
        {
            Debug.Log("inlay gemstone:" + obj);
            PlayerProfile.Data.SetGems(obj["gems"].ToObject<List<Gemstone>>());
        }
    }
}