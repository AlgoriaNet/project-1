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
            // Use the new dot-reading method instead of the old data-based method
            inlayGemstones.InitFromDots(gemstone.Part, sidekickId);
            popup.SetActive(true);
        }
        
        /// <summary>
        /// Public method to refresh the InlayGemstones UI
        /// </summary>
        public void RefreshInlayGemstones(string equipmentPart, int? sidekickId)
        {
            if (inlayGemstones != null)
            {
                Debug.Log($"[GemDetailWithInlaid] Refreshing InlayGemstones for {equipmentPart}");
                inlayGemstones.InitFromDots(equipmentPart, sidekickId);
            }
            else
            {
                Debug.LogWarning($"[GemDetailWithInlaid] inlayGemstones is null, cannot refresh");
            }
        }
    }
}