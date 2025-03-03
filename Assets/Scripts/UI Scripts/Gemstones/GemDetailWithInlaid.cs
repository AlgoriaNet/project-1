using System;
using System.Collections.Generic;
using model;
using UnityEngine;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones
{
    public class GemDetailWithInlaid : MonoBehaviour
    {
        [SerializeField] private GemDetail gemDetail;
        [SerializeField] private InlayGemstones inlayGemstones;
        [SerializeField] private GameObject popup;
        private List<Gemstone> _gems;
        public static GemDetailWithInlaid Instance;

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
            PlayerProfile.Data.AddListener((arg0 => popup.SetActive(false)), "Bag");
        }

        public void Init(Gemstone gemstone, int? sidekickId)
        {
            gemDetail.Init(gemstone, sidekickId);
            if (sidekickId != null)
            {
                _gems = PlayerProfile.Data.GetSidekickGemstones((int)sidekickId, gemstone.Part);
            }
            else
            {
                _gems = PlayerProfile.Data.GetHeroGemstones(gemstone.Part);
            }
            inlayGemstones.Init(_gems);
            popup.SetActive(true);
        }
    }
}